#region license
// Copyleft (-) 2009-2017 Mikael Egevig (mikael@egevig.org).  Donated to the Public Domain.
// 
// Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following 
// conditions are met:
// 
//     * Redistributions of source code must retain the above copyright notice, this list of conditions and the disclaimer below.
//     * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following 
//       disclaimer in the documentation and/or other materials provided with the distribution.
//     * Neither the name of Mikael Egevig nor the names of its contributors may be used to endorse or promote products derived 
//       from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, 
// BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT
// SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING 
// NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
#endregion

//****************************************************************************
// Thoughts on option parsing:
//
//     1. We need a database for the values: Dictionary<name, value>.
//     2. We need to map from a long name to a value.
//     3. We need to map from a short name to a value.
//     4. We need to be able to parse merged short options: rm -fr foo.
//     5. The values must exist separate from the names because multiple
//        options can be referring to the same value.  For instance, -foo and
//        -nofoo both modify the same option value, and sometimes GNU defines
//        three or four options for the same value (and GNU compatibility is
//        still not ruled out!).
//     6. Positional parameters are treated like options with a position
//        instead of a name.
//
// I AM aware that this module is very crude, but it gets the job done.  I am
// still so new to C# that I don't know if the .NET library already includes
// generic wrappers around all types or if the Value hierarchy of types is
// really necessary.  When I get to know C# better, I'll probably revise this
// module.
//
// I am also aware that this module belongs in the category of "worst code I
// ever wrote", but it is difficult to make something generic while conforming
// to the GNU/POSIX way of parsing parameters.  My best recommendation is to
// NOT alter anything unless you run into a very severe bug.  I am pretty
// confident that with so complex and muddy code there's sure to hide a number
// of bugs in here, but I am sick and tired of playing around with this module.
// All I want is a generic option parser that can handle all the cases that
// exist in	Nutbox without any problems.  And it appears to do so.
//
// This is a GENERIC option parser that handles everything Nutbox currently
// needs.  It may be freely reused in other projects.
//
// The Test() method illustrates the use of the class: first a list of options
// are given, then a list of parameters (non-options) are given, and finally
// the whole shebang is thrown at the OptionParser.Add() method, which makes
// sense of it all and parses according to the rules laid out in the Options[]
// array.
//*****************************************************************************

namespace Org.Egevig.Nutbox.Options
{
	using System.Collections.Generic;	// Dictionary<T1, T2>

	/// <summary>
	/// The Value class serves as a base class for all the kinds of values that
	/// the OptionParser class can handle.  I am confident that this could be
	/// done in a more C#-like manner, but I am still too new at the language
	/// to do it any other way.
	/// </summary>
	public abstract class Value
	{
		/// <summary>
		/// The Set() method basically assigns a value to the object.  It
		/// serves as a generic way of assigning values to arbitrary objects.
		/// </summary>
		/// <param name="value">The value to be assigned to the instance.</param>
		/// <returns>Nothing.</returns>
		public abstract void Set(object value);
	}

	/// <summary>
	/// I finally got around to look up generics/templates in C#.  Seems
	/// sensible enough.  C# is a nice language, indeed.
	/// </summary>
	public class TAtomicValue<Type>: Value
	{
		private Type mValue;
		public Type Value
		{
			get { return mValue; }
		}

		public TAtomicValue(Type value)
		{
			mValue = value;
		}

		public override void Set(object value)
		{
			mValue = (Type) value;
		}
	}

	public class BooleanValue: TAtomicValue<bool>
	{
		public BooleanValue(bool value) :
			base(value)
		{
		}
	}

	public class StringValue: TAtomicValue<string>
	{
		public StringValue(string value) :
			base(value)
		{
		}
	}

	public class IntegerValue: TAtomicValue<int>
	{
		public IntegerValue(int value) :
			base(value)
		{
		}
	}

	public class LongValue: TAtomicValue<long>
	{
		public LongValue(long value) :
			base(value)
		{
		}
	}

	public class DateTimeValue: TAtomicValue<System.DateTime>
	{
		public DateTimeValue(System.DateTime value) :
			base(value)
		{
		}
	}

	public class ListValue: Value
	{
		private List<string> _value = new List<string>();
		public List<string> Value
		{
			get { return _value; }
		}

		public override void Set(object value)
		{
			_value.Add((string) value);
		}
	}

	/// <summary>
	/// This is the base class for all options - whether positional or named.
	/// It defines a single method, Parse(), which is responsible for parsing
	/// a given string and returning the index of the last char parsed.  This
	/// is used when parsing merged options (-fr) and it is utterly crucial
	/// that Parse() returns the index of the last character parsed.
	/// </summary>
	public abstract class Option
	{
		// LIST is added to Code when the parameter is a list of something
		public const int LIST = 65536;  // arbitrary set bit

		/// <summary>
		/// Code is the index of the positional parameter.  If the parameter in
		/// question is an option, the code is zero.  In other words: Each
		/// positional parameter is assigned a positive integer corresponding
		/// to its position in the parameter stream, whereas options are
		/// assigned the position zero (0) to indicate that it has no fixed
		/// position in the input stream of parameters.
		/// </summary>
		private int mCode;				// m means "member" (CLS compatible)
		public int Code
		{
			get { return mCode; }
		}

		private eMode mMode;
		public eMode Mode
		{
			get { return mMode; }
		}

		/// <summary>
		/// The name of the option: Either a single character, in which case it
		/// is case-sensitive, or two characters or more, in which case it is
		/// case-insensitive.  This is to match GNU behavior.
		/// </summary>
		private string mName;
		public string Name
		{
			get { return mName; }
		}

		/// <summary>
		/// Indicates whether the option parses anything or not.  If true, the
		/// main parser, Parser, knows to expect parameters for the option
		/// or parameter in question.  If false, it will not expect any
		/// parameters to the option in question, but Parse() will still be
		/// invoked (so as to set the value in question).
		/// </summary>
		private bool mParses;
		public bool Parses
		{
			get { return mParses; }
		}

		private Value mValue;
		public Value Value
		{
			get { return mValue; }
			set { mValue = value; }
		}

		/// <summary>
		/// The mode of the option: Optional, Mandatory or EndOfOptions.  The
		/// last, EndOfOptions, indicates that the option is also Mandatory.
		/// </summary>
		public enum eMode			// e = "enumeration"
		{
			Optional,				// this parameter is optional
			Mandatory,				// this parameter is mandatory
			EndOfOptions			// marks the end of options (also mandatory)
		};

		protected Option(int code, string name, Value value, bool parses, eMode mode)
		{
			if (code < 0)
				throw new Nutbox.InternalError("Invalid code specified " + code.ToString());
			if (code == 0 && mode != eMode.Optional)
				throw new Nutbox.InternalError("An option must be optional: " + name);

			mCode   = code;			// 0 = real option, 1+ = parameter
			mName   = name;
			mValue  = value;
			mParses = parses;
			mMode   = mode;
		}

		public abstract int Parse(string value, int index);
	}

	/// <summary>
	/// This covers the case of the user needing to specify a date-time value.
	/// It uses Nutbox's standard date-time format, which is culture neutral.
	/// </summary>
	public class DateTimeOption: Option
	{
		public DateTimeOption(string name, Value value) :
			base(0, name, value, true, eMode.Optional)
		{
		}

		public override int Parse(string value, int index)
		{
			System.DateTime result;

			if (!Nutbox.Platform.Time.TryParse(value.Substring(index), out result))
				throw new Nutbox.Exception("Invalid time specified: " + value);

			Value.Set((object) result);
			return value.Length;
		}
	}

	/// <summary>
	/// Occasionally, it is useful to assign a date-time value using an option.
	/// For instance, the 'touch' command has its --notime option, which
	/// restores the original default value when used.
	/// </summary>
	public class DateTimeConstantOption: Option
	{
		private System.DateTime _constant;

		public DateTimeConstantOption(
			string name,
			DateTimeValue value,
			System.DateTime constant
		) : base(0, name, value, true, eMode.Optional)
		{
			_constant = constant;
		}

		public override int Parse(string value, int index)
		{
			Value.Set((object) _constant);
			return index;
		}
	}

	/// <summary>
	/// An option that yields a true value.  The most used kind of option.
	/// </summary>
	public class TrueOption: Option
	{
		public TrueOption(string name, BooleanValue value) :
			base(0, name, value, false, eMode.Optional)
		{
		}

		public override int Parse(string value, int index)
		{
			Value.Set(true);
			return index;
		}
	}

	/// <summary>
	/// An option that yields a false value.  The next most used option.
	/// </summary>
	public class FalseOption: Option
	{
		public FalseOption(string name, BooleanValue value) :
			base(0, name, value, false, eMode.Optional)
		{
		}

		public override int Parse(string value, int index)
		{
			Value.Set(false);
			return index;
		}
	}

	/// <summary>
	/// An integer option.  All Nutbox integer parameters and options support
	/// a size suffix (kb, mb, gb, and tb).
	/// </summary>
	public class IntegerOption: Option
	{
		public IntegerOption(string name, IntegerValue value) :
			base(0, name, value, true, eMode.Optional)
		{
		}

		public override int Parse(string value, int index)
		{
			long result = Nutbox.Types.Long.Parse(value.Substring(index));
			if (result < System.Int32.MinValue || result > System.Int32.MaxValue)
				throw new Nutbox.Exception("Integer out of range: " + value.Substring(index));

			Value.Set((object) (int) result);
			return value.Length;
		}
	}

	/// <summary>
	/// Occasionally, there's a need to let an option assign a constant value.
	/// </summary>
	public class IntegerConstantOption: Option
	{
		private int mConstant;

		public IntegerConstantOption(string name, IntegerValue value, int constant) :
			base(0, name, value, true, eMode.Optional)
		{
			mConstant = constant;
		}

		public override int Parse(string value, int index)
		{
			Value.Set((object) mConstant);
			return index;
		}
	}

	/// <summary>
	/// This is for advanced stuff: A callback that can handle all cases.  As
	/// the handler is typically defined in the derived Setup() class, it can
	/// freely munge about with all the options defined and thus implement
	/// whatever you need it to implement.  It is currently only used in
	/// 'nutmake', where it has the task of importing a symbol from the
	/// environment.
	/// </summary>
	public class DelegateOption: Option
	{
		public delegate int Handler(string name, int index);
		Handler mHandler;

		public DelegateOption(string name, Handler handler, bool parses) :
			base(0, name, null, parses, eMode.Optional)
		{
			mHandler = handler;
		}

		public override int Parse(string value, int index)
		{
			return mHandler(value, index);
		}
	}

	/// <summary>
	/// A long option or parameter value.  Supports size suffixes.
	/// </summary>
	public class LongOption: Option
	{
		public LongOption(string name, LongValue value) :
			base(0, name, value, true, eMode.Optional)
		{
		}

		public override int Parse(string value, int index)
		{
			long result = Nutbox.Types.Long.Parse(value.Substring(index));
			Value.Set((object) result);
			return value.Length;
		}
	}

	/// <summary>
	/// The corresponding long constant option, which simply assigns the
	/// specified constant to the specified value if invoked.
	/// </summary>
	public class LongConstantOption: Option
	{
		private long mConstant;

		public LongConstantOption(string name, LongValue value, long constant) :
			base(0, name, value, false, eMode.Optional)
		{
			mConstant = constant;
		}

		public override int Parse(string value, int index)
		{
			Value.Set((object) mConstant);
			return index;
		}
	}

	/// <summary>
	/// A quite frequently used option type.
	/// </summary>
	public class StringOption: Option
	{
		public StringOption(string name, StringValue value) :
			base(0, name, value, true, eMode.Optional)
		{
		}

		public override int Parse(string value, int index)
		{
			Value.Set((object) value.Substring(index));
			return value.Length;
		}
	}

	/// <summary>
	/// A string constant assignment.  Useful for restoring defaults.
	/// </summary>
	/// <returns></returns>
	public class StringConstantOption: Option
	{
		private string mConstant;

		public StringConstantOption(string name, StringValue value, string constant) :
			base(0, name, value, false, eMode.Optional)
		{
			mConstant = constant;
		}

		public override int Parse(string value, int index)
		{
			Value.Set((object) mConstant);
			return index;
		}
	}

	/// <summary>
	/// A string positional parameter (i.e. not an option).  Useful for almost
	/// everything, although the basic idea with this option parser is that
	/// the user should derive from Option and implement his or her own special
	/// option parsers as the need arise.
	/// </summary>
	/// <returns></returns>
	public class StringParameter: Option
	{
		public StringParameter(int code, string name, StringValue value, eMode mode) :
			base(code, name, value, true, mode)
		{
		}

		public override int Parse(string value, int index)
		{
			Value.Set(value.Substring(index));
			return value.Length;
		}
	}

	/// <summary>
	/// A long parameter - supports size suffixes.
	/// </summary>
	/// <returns></returns>
	public class LongParameter: Option
	{
		public LongParameter(int code, string name, LongValue value, eMode mode) :
			base(code, name, value, true, mode)
		{
		}

		public override int Parse(string value, int index)
		{
			long result = Nutbox.Types.Long.Parse(value.Substring(index));

			Value.Set((object) result);
			return value.Length;
		}
	}

	/// <summary>
	/// A list of string options.
	/// </summary>
	/// <returns></returns>
	public class ListOption: Option
	{
		public ListOption(string name, ListValue value) :
			base(0, name, value, true, eMode.Optional)
		{
		}

		public override int Parse(string value, int index)
		{
			Value.Set(value.Substring(index));
			return value.Length;
		}
	}

	/// <summary>
	/// A list of string parameter - used all over the place.
	/// </summary>
	/// <returns></returns>
	public class ListParameter: Option
	{
		public ListParameter(int code, string name, ListValue value, eMode mode) :
			base(code + LIST, name, value, true, mode)
		{
		}

		public override int Parse(string value, int index)
		{
			Value.Set(value.Substring(index));
			return value.Length;
		}
	}

	/// <summary>
	/// The option parser, Parser, is responsible for parsing the entire
	/// command-line, which is typically passed to it using the Parse(string[])
	/// method.  The command-line syntax is defined using a list of options.
	/// The command-line parser is pretty flexible but in return also quite
	/// long-haired.
	/// </summary>
	public class Parser
	{
		private const int UNUSED = 100000;

		private bool _ignore = false;		// true => ignore remaining options
		private int _next = 1;				// index of next positional parameter
		private Dictionary<string, Option> _names = new Dictionary<string, Option>();
		private Dictionary<int, Option>    _codes = new Dictionary<int, Option>();
		private Option _short = null;		// !null => short option that needs arg
		private int mOptional = UNUSED;		// index of first optional parameter (0 = none)
		private int mList     = UNUSED;		// index of list item, if any
		private int mLast     = 0;

		/// <summary>
		/// Check() is invoked by Nutbox.Program.Main() once all the arguments
		/// have been parsed by Parse(string[]).  It checks that option parsing
		/// did not terminate in the middle of an option and that all required
		/// options have been processed.
		/// Remember to invoke the base's Check() method if you override it!
		/// </summary>
		/// <returns>Nothing.</returns>
		public virtual void Check()
		{
			// check if a short option awaits its argument
			if (_short != null)
				throw new Nutbox.Exception("Parameter missing in option: -" + _short.Name);

			// we got all the args we need, no more fuzzying about it
			if (_next >= mLast + 1)
				return;

			// if not all parameters were parsed, check if they are optional
			if (_next <= mLast && _codes[_next].Mode != Option.eMode.Optional)
				throw new Nutbox.Exception("Missing parameter: " + _codes[_next].Name);
		}

		public void Add(Option[] options)
		{
			foreach (Option option in options)
				Add(option);

			// todo: check that there are no gaps in positional parameters
			// todo: (although this will quickly be discovered by the test)
		}

		public void Add(Option option)
		{
			string name;
			if (option.Name.Length == 1)
				name = option.Name;
			else
				name = option.Name.ToUpperInvariant();

			// check for duplicate options
			if (_names.ContainsKey(name))
				throw new Nutbox.InternalError("Duplicate option detected: " + name);

			// register the option in our tiny "database" of options
			_names[name] = option;

			// a non-positional parameter (an option), so we're done
			if (option.Code == 0)
				return;

			int code = option.Code & ~Option.LIST;	// strip LIST bit

			if (code != mLast + 1)
				throw new Nutbox.Exception("Missing parameter definition: " + (mLast + 1).ToString());
			mLast = code;

			// record the position of the first optional parameter, if any
			if (option.Mode >= Option.eMode.Mandatory)
			{
				if (mOptional != UNUSED)
					throw new Nutbox.Exception("Mandatory parameter follows optional parameter: " + option.Name);
			}
			else
				mOptional = code;

			// record the position of the first list, if any
			if ((option.Code & Option.LIST) != 0)
			{
				if (mList != UNUSED)
					throw new Nutbox.Exception("Two lists defined: " + option.Name);
				mList = code;
			}

			// check for duplicate positional parameters
			if (_codes.ContainsKey(code))
				throw new Nutbox.InternalError("Duplicate parameter detected: " + option.Code.ToString());

			// finally, register it for easy look up later on
			_codes[code] = option;
		}

		public void Parse(string[] args)
		{
			foreach (string arg in args)
				Parse(arg);
		}

		// parses a command-line parameter
		// long-haired code follows, but I am new to C#...
		public virtual void Parse(string arg)
		{
			// reject empty parameters
			if (arg.Length == 0)
				throw new Nutbox.Exception("Empty parameter encountered");

			// if parsing missing parameter to short option, grab it right away
			if (_short != null)
			{
				int i = _short.Parse(arg, 0);
				if (i != arg.Length)
					throw new Nutbox.Exception("Extranous data in option: " + arg);

				_short = null;
				return;
			}

			// a response file
			if (arg[0] == '@')
			{
				System.IO.StreamReader reader = new System.IO.StreamReader(arg.Substring(1));
				Parse(reader);
				reader.Close();
				return;
			}

			// pseudo-option that disables further option parsing
			if (arg == "--")
			{
				_ignore = true;
				return;
			}

			// a long option
			if (!_ignore && arg.Length > 2 && arg[0] == '-' && arg[1] == '-')
			{
				SplitResult parts;
				parts = SplitLongOption(arg);

				if (!_names.ContainsKey(parts.Name))
					throw new Nutbox.Exception("Unknown option: " + arg);

				if (parts.Name.Length == 1)
					throw new Nutbox.Exception("Short option used as long option: " + arg);

				// only accept non-positional parameters as options
				Option option = _names[parts.Name];
				if (option.Code != 0)
					throw new Nutbox.Exception("Unknown option: " + arg);

				// check that there's anything to parse, if applicable
				if (option.Parses && parts.Data == null)
					throw new Nutbox.Exception("Missing parameter in option: " + arg);

				// check that there's nothing to parse, if applicable
				if (!option.Parses && parts.Data != null)
					throw new Nutbox.Exception("Extranous parameter in option: " + arg);

				// parse and check that everything got parsed properly
				int index = option.Parse(parts.Data, 0);
				if (parts.Data != null && index != parts.Data.Length)
					throw new Nutbox.Exception("Extranous data in option: " + arg);

				return;
			}

			// a sequence of short options
			if (!_ignore && arg[0] == '-')
			{
				if (arg.Length == 1)
					throw new Nutbox.Exception("Malformed short option: " + arg);

				int i = 1;
				while (i < arg.Length)
				{
					// extract the name and parse the argument, if any
					// note: short options are case-sensitive, unlike the long
					// note: options.  At least that's how Nutbox does it...
					string name = "" + arg[i];

					// check that it is a valid option
					if (!_names.ContainsKey(name))
						throw new Nutbox.Exception("Unknown option: -" + arg[i]);
					Option option = _names[name];

					// only accept non-positional parameters as options
					if (option.Code != 0)
						throw new Nutbox.Exception("Unknown option: -" + arg[i]);

					// if there is nothing to parse, parse it next time round
					if (i + 1 == arg.Length && option.Parses)
					{
						_short = option;
						return;
					}

					i = option.Parse(arg, i + 1);
				}

				return;
			}

			// a positional parameter
			{
				// if we're parsing a list, stay with the index of its definition
				int idx = (_next > mList) ? mList : _next;

				if (!_codes.ContainsKey(idx))
					throw new Nutbox.Exception("Extranous parameter: " + arg);
				Option option = _codes[idx];
				int i = option.Parse(arg, 0);
				if (i != arg.Length)
					throw new Nutbox.InternalError("Misparsed parameter detected: " + arg);
				_ignore |= (option.Mode == Option.eMode.EndOfOptions);
				_next += 1;
			}
		}

		// Parse:
		// Parses a response (command) file by treating each line as a parameter.
		public void Parse(System.IO.TextReader reader)
		{
			// iterate over all lines in the input file
			for (;;)
			{
				// read a single line at a time (one line equals one parameter)
				string line = reader.ReadLine();
				if (line == null)
					break;

				// discard leading and trailing white-space, just in case
				string trimmed = line.Trim();

				// ignore empty lines
				if (trimmed.Length == 0)
					continue;

				// ignore comments in response files
				if (trimmed[0] == '#')
					continue;

				Parse(trimmed);
			}
		}

		public struct SplitResult
		{
			private string _name;
			public string Name
			{
				get { return _name; }
				set { _name = value; }
			}

			private string _data;
			public string Data
			{
				get { return _data; }
				set { _data = value; }
			}
		}

		private static int min(int a, int b)
		{
			return (a < b) ? a : b;
		}

		public static int FindDelimiter(string arg)
		{
			// note: we accept both ':' and '=', picking the one that comes
			// note: first.  This way we're compatible with both GNU and M$.
			// Cumbersome, but true.
			int pos1 = arg.IndexOf(':');
			int pos2 = arg.IndexOf('=');
			int pos;
			if (pos1 != -1 && pos2 != -1)
				pos = min(pos1, pos2);
			else if (pos1 != -1)
				pos = pos1;
			else if (pos2 != -1)
				pos = pos2;
			else
				pos = -1;
			return pos;
		}

		public static SplitResult SplitLongOption(string arg)
		{
			SplitResult result = new SplitResult();

			// parse option into its name and data parts
			int pos = FindDelimiter(arg);
			if (pos == -1)
			{
				result.Name = arg.Substring(2);	// skip leading dashes
				result.Data = null;
			}
			else
			{
				result.Name = arg.Substring(2, pos - 2); // skip leading dashes and trailing colon
				result.Data = arg.Substring(pos + 1);
			}

			// cumbersome but FxCop v1.36 insists on me mending my ways...
			// I strongly dislike all those UPPERCASE strings, but M$ says so.
			result.Name = result.Name.ToUpperInvariant();

			return result;
		}

		public static SplitResult SplitShortOption(string arg)
		{
			SplitResult result = new SplitResult();

			int pos = FindDelimiter(arg);
			if (pos == -1)
			{
				result.Name = arg.Substring(1);  // skip leading dash
				result.Data = null;
			}
			else
			{
				result.Name = arg.Substring(1, pos - 1);
				result.Data = arg.Substring(pos + 1);
			}

			if (result.Name.Length != 1)
				throw new Nutbox.Exception("Malformed short option: " + arg);

			// note: short options are CASE-SENSITIVE so don't uppercase them.

			return result;
		}

		public static void Test()
		{
			BooleanValue test    = new BooleanValue(false);
			StringValue filename = new StringValue(null);
			IntegerValue lines   = new IntegerValue(10);
			ListValue names      = new ListValue();
			ListValue strings    = new ListValue();

			Option[] options =
			{
				new TrueOption("t", test),
				new FalseOption("notest", test),
				new TrueOption("test", test),
				new IntegerOption("lines", lines),
				new ListOption("name", names),
				new StringParameter(1, "filename", filename, Option.eMode.EndOfOptions),
				new ListParameter(2, "strings", strings, Option.eMode.Mandatory),
			};

			Parser parser = new Parser();
			parser.Add(options);

			System.Console.WriteLine("Test = {0}", test.Value);
			parser.Parse("-t");
			System.Console.WriteLine("Test = {0}", test.Value);
			parser.Parse("--test");
			System.Console.WriteLine("Test = {0}", test.Value);
			parser.Parse("--notest");
			System.Console.WriteLine("Test = {0}", test.Value);
			parser.Parse("-tt");
			System.Console.WriteLine("Test = {0}", test.Value);
			parser.Parse("--notest");
			System.Console.WriteLine("Test = {0}", test.Value);

			parser.Parse("--name:first");
			parser.Parse("--name:other");
			foreach (string name in names.Value)
			{
				System.Console.WriteLine("Names = {0}", name);
			}

			parser.Parse("foobar.txt");
			System.Console.WriteLine("Filename = {0}", filename.Value);

			parser.Parse("string1");
			parser.Parse("string2");
			parser.Parse("string3");
			parser.Parse("string4");
			parser.Parse("--treated-as-a-positional-parameter");
			foreach (string str in strings.Value)
			{
				System.Console.WriteLine("Strings = {0}", str);
			}
		}
	}
}
