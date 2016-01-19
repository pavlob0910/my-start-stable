using System;
using System.Threading.Tasks;

using System.Net.Http;
using System.Text;
using System.Collections.Generic;

namespace MyStarStable.Common
{
	public class Newsheadline
	{
		public string href { get; set; }
		public string text { get; set; }
	}

	public class Collection1
	{
		public Newsheadline newsheadline { get; set; }
		public int index { get; set; }
		public string url { get; set; }
	}

	public class Results
	{
		public List<Collection1> collection1 { get; set; }
	}

	public class RootObject
	{
		public string name { get; set; }
		public int count { get; set; }
		public string frequency { get; set; }
		public int version { get; set; }
		public bool newdata { get; set; }
		public string lastrunstatus { get; set; }
		public string thisversionstatus { get; set; }
		public string nextrun { get; set; }
		public string thisversionrun { get; set; }
		public Results results { get; set; }
	}
}

