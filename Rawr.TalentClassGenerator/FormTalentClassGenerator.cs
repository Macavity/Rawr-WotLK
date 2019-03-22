﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using System.Globalization;

namespace Rawr.TalentClassGenerator
{
	public partial class FormTalentClassGenerator : Form
	{
		public FormTalentClassGenerator()
		{
			InitializeComponent();
		}

		private void buttonGenerateCode_Click(object sender, EventArgs e)
		{
            textBoxCode.Text = @"using System;
using System.Text;
using System.Collections.Generic;

namespace Rawr
{
    /// <summary>
    /// This file is automatically generated by the Rawr.TalentClassGenerator tool. Please don't edit it directly.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class TalentDataAttribute : Attribute
    {
        public TalentDataAttribute(int index, string name, int maxPoints, int tree, int column, int row, int prerequisite, string[] description, string icon)
        {
            _index = index;
            _name = name;
            _maxPoints = maxPoints;
            _tree = tree;
            _column = column;
            _row = row;
            _prerequisite = prerequisite;
            _description = description;
            _icon = icon;
        }

        private readonly int _index;
        private readonly string _name;
        private readonly int _maxPoints;
        private readonly int _tree;
        private readonly int _column;
        private readonly int _row;
        private readonly int _prerequisite;
        private readonly string _icon;
        private readonly string[] _description;

        public int Index { get { return _index; } }
        public string Name { get { return _name; } }
        public int MaxPoints { get { return _maxPoints; } }
        public int Tree { get { return _tree; } }
        public int Column { get { return _column; } }
        public int Row { get { return _row; } }
        public int Prerequisite { get { return _prerequisite; } }
        public string[] Description { get { return _description; } }
        public string Icon { get { return _icon; } }
    }

    public abstract class TalentsBase
    {
        public abstract int[] Data { get; }
        public virtual bool[] GlyphData { get { return null; } }

        protected void LoadString(string code)
        {
            if (string.IsNullOrEmpty(code)) return;
            int[] _data = Data;
            string[] tmp = code.Split('.');
            string talents = tmp[0];
            if (talents.Length >= _data.Length)
            {
                List<int> data = new List<int>();
                foreach (Char digit in talents)
                    data.Add(int.Parse(digit.ToString()));
				data.CopyTo(0, _data, 0, _data.Length);
            }
            if (tmp.Length > 1)
            {
                string glyphs = tmp[1];
                bool[] _glyphData = GlyphData;
                if (_glyphData != null && glyphs.Length == _glyphData.Length)
                {
                    List<bool> data = new List<bool>();
                    foreach (Char digit in glyphs)
                        data.Add(int.Parse(digit.ToString()) == 1);
                    data.CopyTo(_glyphData);
                }
            }
        }

        public override string ToString()
        {
            StringBuilder ret = new StringBuilder();
            foreach (int digit in Data)
                ret.Append(digit.ToString());
            if (GlyphData != null)
            {
                ret.Append('.');
                foreach (bool glyph in GlyphData)
                {
                    ret.Append(glyph ? '1' : '0');
                }
            }
            return ret.ToString();
        }

        public CharacterClass GetClass()
        {
            if (GetType() == typeof(WarlockTalents)) return CharacterClass.Warlock;
            if (GetType() == typeof(MageTalents)) return CharacterClass.Mage;
            if (GetType() == typeof(PriestTalents)) return CharacterClass.Priest;
            if (GetType() == typeof(DruidTalents)) return CharacterClass.Druid;
            if (GetType() == typeof(RogueTalents)) return CharacterClass.Rogue;
            if (GetType() == typeof(HunterTalents)) return CharacterClass.Hunter;
            if (GetType() == typeof(ShamanTalents)) return CharacterClass.Shaman;
            if (GetType() == typeof(DeathKnightTalents)) return CharacterClass.DeathKnight;
            if (GetType() == typeof(PaladinTalents)) return CharacterClass.Paladin;
            return CharacterClass.Warrior;
        }

#if RAWR3
		public abstract TalentsBase Clone();
#endif
    }

";

    //<talentTab classId="2" name="Paladin" url="c=Paladin"/>
    //<talentTab classId="4" name="Rogue" url="c=Rogue"/>
    //<talentTab classId="9" name="Warlock" url="c=Warlock"/>
    //<talentTab classId="8" name="Mage" url="c=Mage"/>
    //<talentTab classId="6" name="Death Knight" url="c=Death+Knight"/>
    //<talentTab classId="11" name="Druid" url="c=Druid"/>
    //<talentTab classId="1" name="Warrior" url="c=Warrior"/>
    //<talentTab classId="3" name="Hunter" url="c=Hunter"/>
    //<talentTab classId="7" name="Shaman" url="c=Shaman"/>
    //<talentTab classId="5" name="Priest" url="c=Priest"/>

            int[] classId = new [] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 11 };
            string[] className = new [] { "Warrior", "Paladin", "Hunter", "Rogue", "Priest", "DeathKnight", "Shaman", "Mage", "Warlock", "Druid" };

            for (int i = 0; i < classId.Length; i++)
            {
                ProcessTalentDataXml(new StreamReader(System.Net.HttpWebRequest.Create(
                    string.Format(textBoxUrl.Text, classId[i])).GetResponse().GetResponseStream()).ReadToEnd(), className[i]);
            }
			textBoxCode.Text += "}";
			textBoxCode.SelectAll();
			textBoxCode.Focus();
		}

        private void ProcessTalentDataXml(string fullResponse, string className)
        {
            XmlDocument talentDoc = new XmlDocument();
            talentDoc.LoadXml(fullResponse);
            XmlNode[] trees = new XmlNode[3];
            List<TalentData> talents = new List<TalentData>();
            className = className + "Talents";

            foreach (XmlNode tree in talentDoc.SelectNodes("page/talentTrees/tree"))
            {
                trees[int.Parse(tree.Attributes["order"].Value)] = tree;
            }
            Dictionary<string, TalentData> talentDictionary = new Dictionary<string, TalentData>();

            for (int i = 0; i < trees.Length; i++)
            {
                XmlNode tree = trees[i];
                for (int row = 0; row <= 10; row++)
                {
                    foreach (XmlNode talent in tree.SelectNodes("talent[@tier = " + row + "]"))
                    {
                        XmlNodeList ranks = talent.SelectNodes("rank");
                        TalentData talentData = new TalentData()
                        {
                            Index = talents.Count,
                            Name = talent.Attributes["name"].Value,
                            MaxPoints = ranks.Count,
                            Tree = i,
                            Icon = talent.Attributes["icon"].Value,
                            Column = int.Parse(talent.Attributes["column"].Value) + 1,
                            Row = row + 1,
                            Prerequisite = null,
                            Description = new string[ranks.Count]
                        };
                        talentDictionary[talent.Attributes["key"].Value] = talentData;
                        XmlAttribute requires = talent.Attributes["requires"];
                        if (requires != null)
                        {
                            talentData.Prerequisite = requires.Value;
                        }
                        foreach (XmlNode rank in ranks)
                        {
                            talentData.Description[int.Parse(rank.Attributes["lvl"].Value) - 1] = rank.Attributes["description"].Value;
                        }
                        talents.Add(talentData);
                    }
                }
            }

            //Generate the code
            StringBuilder code = new StringBuilder();
            code.AppendFormat(@"public partial class {0} : TalentsBase
#if RAWR3
	{{
		public override TalentsBase Clone()
#else
		, ICloneable 
	{{
        public {0} Clone() {{ return ({0})((ICloneable)this).Clone(); }}
		object ICloneable.Clone()
#endif	
		{{
			{0} clone = ({0})MemberwiseClone();
			clone._data = (int[])_data.Clone();
			clone._glyphData = (bool[])_glyphData.Clone();
			return clone;
		}}
", className);
            code.Append("\r\n");
            code.AppendFormat("private int[] _data = new int[{0}];\r\n", talents.Count);
            code.Append("public override int[] Data { get { return _data; } }\r\n");
            code.AppendFormat("public {0}() {{ }}\r\n", className);
            code.AppendFormat("public {0}(string talents)\r\n", className);
            code.Append("{\r\n");
            code.Append("LoadString(talents);\r\n");
            code.Append("}\r\n");
            code.Append("public static string[] TreeNames = new [] {");
            foreach (XmlNode tree in trees)
                code.AppendFormat("\r\n@\"{0}\",", tree.Attributes["name"].Value);
            code.Append("};\r\n\r\n");
            foreach (TalentData talent in talents)
            {
                code.Append(GenerateComment(talent));
                code.AppendFormat("\r\n[TalentData({0}, \"{1}\", {2}, {3}, {4}, {5}, {6}, new [] {{",
                    talent.Index, talent.Name, talent.MaxPoints, talent.Tree, talent.Column, talent.Row, talent.Prerequisite == null ? -1 : talentDictionary[talent.Prerequisite].Index);
                foreach (string descRank in talent.Description)
                {
                    //strip html breaks from descriptions
                    string description = descRank.Replace("<br/>", "\r\n");
                    code.AppendFormat("\r\n@\"{0}\",", description);
                }
                code.Append("}, \"" + talent.Icon + "\")]\r\n");
                code.AppendFormat("public int {0} {{ get {{ return _data[{1}]; }} set {{ _data[{1}] = value; }} }}\r\n",
                    PropertyFromName(talent.Name), talent.Index);
            }
            code.Append("}\r\n\r\n");

            textBoxCode.Text += code.ToString();
        }

        /// <summary>
        /// Generate a comment for the talent field, based on it's description( Replaceed changed value by [BaseNumber * Pts])
        /// </summary>
        /// <param name="Talent">Given talent</param>
        /// <returns>The comment</returns>
        private string GenerateComment(TalentData Talent)
        {
            string Comment = Talent.Description[Talent.Description.Length - 1];

            if (Talent.Description.Length > 1)
            {
                char[] SplitCharacter = new char[] { ' ', '%' };
                string[] FirstRank = Talent.Description[0].Split(SplitCharacter, StringSplitOptions.RemoveEmptyEntries);
                string[] LastRank = Talent.Description[Talent.Description.Length - 1].Split(SplitCharacter, StringSplitOptions.RemoveEmptyEntries);

                int ReplacePos = 0;

                //Description contains the same count of words for all ranks, diference only in some values
                for (int i = 0; i < Math.Min(FirstRank.Length,LastRank.Length); i++)
                {
                    if (FirstRank[i] != LastRank[i])
                    {
                        //To avoid string like "... increase by 5."
                        if (FirstRank[i].Contains(".") == false) FirstRank[i] = FirstRank[i] + ".0";
                        else FirstRank[i] = FirstRank[i] + "0";

                        float BaseNumber = 0;
                        if (float.TryParse(FirstRank[i], NumberStyles.Any, new NumberFormatInfo(), out BaseNumber))
                        {
                            float MaxNumber = 0;
                            string Replaced = "[{0} * Pts]";
                            if (float.TryParse(LastRank[i], NumberStyles.Any, new NumberFormatInfo(), out MaxNumber))
                            {
                                int Base = Convert.ToInt32(BaseNumber);
                                int Max = Convert.ToInt32(MaxNumber);

                                //Number like: BaseNumber - 7, MaxNumber - 20
                                if ((Base > 0) && ((Max / Base) * Base) != Max)
                                {
                                    Replaced = "[" + MaxNumber + " / " + Talent.Description.Length + " * Pts]";
                                }
                            }
                            
                            Comment = Replace(Comment, LastRank[i], String.Format(Replaced, BaseNumber), ref ReplacePos);
                        }
                    }
                }
            }

            Comment = @"        /// <summary>" + Environment.NewLine +
                      @"        /// " + Comment + Environment.NewLine +
                      @"        /// </summary>";
            return Comment;
        }

        /// <summary>
        /// Returns a new string in which first occurrences of a specified string after position in this input string are replaced with another specified string.
        /// </summary>
        /// <param name="Text">Input string</param>
        /// <param name="OldValue">A string to be replaced</param>
        /// <param name="NewValue">A string to replace first occurrences of OldValue</param>
        /// <param name="Position">The starting character position, after wich, first occurences be replaced. 
        /// When this method returns, contains the 32-bit signed integer value, indicates the position, after replaced string  </param>
        /// <returns>A String equivalent to the input string but with first instance of OldValue replaced with NewValue</returns>
        private string Replace(string Text, string OldValue, string NewValue, ref int Position)
        {
            int NewPosition = Text.IndexOf(OldValue, Position);
            string Res = Text.Substring(0, NewPosition) + NewValue + Text.Substring(NewPosition + OldValue.Length);

            Position = NewPosition + NewValue.Length;
            return Res;
        }

        private string PropertyFromName(string name)
        {
            name = name.Replace("'", ""); // don't camel word after apostrophe
            string[] arr = name.Split(new char[] {' ', ',', ':', '(', ')', '.', '-'}, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = Char.ToUpperInvariant(arr[i][0]) + arr[i].Substring(1);
            }
            return string.Join("", arr);
        }

        private string GetTextBetween(string text, string start, string end)
		{
			string ret = text.Substring(text.IndexOf(start) + start.Length);
			ret = ret.Substring(0, ret.IndexOf(end));
			return ret;
		}

		private class TalentData
		{
			public int Index { get; set; }
			public string Name { get; set; }
			public int MaxPoints { get; set; }
			public int Tree { get; set; }
			public int Column { get; set; }
			public int Row { get; set; }
            public string Icon { get; set; }
            public string Prerequisite { get; set; }
			public string[] Description { get; set; }
		}
	}
}