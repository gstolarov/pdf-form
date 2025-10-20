using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using iTextSharp.text.pdf;
using iTextSharp.text;
using System.Diagnostics;
using System.Xml;
namespace ProcPdf {
	public partial class cvt : Form {
		public cvt() {
			InitializeComponent();
		}
		private void btnMore_Click(object sender, EventArgs e) {
			dlgFile.FileName = pdfFile.Text;
			dlgFile.Filter = "pdf files (*.pdf)|*.pdf|All files (*.*)|*.*";
			dlgFile.DefaultExt = "*.pdf";
			if (dlgFile.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				pdfFile.Text = dlgFile.FileName;
		}
		void createSvg(string file) {
			//https://github.com/dmester/pdftosvg.net
			FileInfo fi = new FileInfo(file);
			List<string> sbErr = new List<string>(), sbOut = new List<string>();
			Process p = new Process() {
				StartInfo = new ProcessStartInfo() {
					WorkingDirectory = fi.DirectoryName, 
					FileName = Environment.GetEnvironmentVariable("ProgramW6432")
								+ @"\Inkscape\bin\inkscape.com", 
					Arguments = $"-o \"{fi.Name.Substring(0, fi.Name.Length - 4)}.svg\" \"{fi.Name}\"",
					WindowStyle = ProcessWindowStyle.Hidden, CreateNoWindow = true,
					UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true
				}
			};
			p.OutputDataReceived += (s, e) => { if (!e.Data.IsEmpty()) sbOut.Add(e.Data); };
			p.ErrorDataReceived += (s, e) => { if (!e.Data.IsEmpty()) sbErr.Add(e.Data); };
			p.Start();
			p.BeginOutputReadLine();
			p.BeginErrorReadLine();
			p.WaitForExit();
			if (p.ExitCode != 0 || string.Join("\n", sbErr).Trim().Length > 0)
				throw new Exception("Failed to exec InkScape");
		}

		class Fld {
			public string name, alt, type = "", db, align, chkVal = "", mask;
			public float top, left, width, height, szFont = 0;
			public float right {  get { return left + width; } }
			public float btm { get { return top + height; } }
			public override string ToString() {
				StringBuilder sb = new StringBuilder("\t\t<input ");
				if (!type.IsEmpty()) sb.Append("type='" + type + "' ");
				sb.Append("id='" + name + "' ");
				if (!alt.IsEmpty()) 
					sb.Append("title='").Append(XmlConvert.EncodeName(alt)).Append("' ");
				sb.Append(" left='").Append(left).Append("' top='").Append(top).Append("' width='")
							.Append(width).Append("' height='").Append(height).Append("' />\n");
				return sb.ToString();
			}
		}
		void updateSvgFile(int pg, string flds, int pw, int ph) {
			FileInfo fi = new FileInfo(pdfFile.Text);
			string fName = fi.Name.Substring(0, fi.Name.Length - 4);
			XmlDocument xml = new XmlDocument();
			string svgFile = string.Format(@"{0}\{1}_Page{2}.svg", fi.DirectoryName, fName, pg);
			xml.Load(svgFile);
			XmlNode meta = xml.SelectSingleNode("/*/*[local-name()='metadata']");
			if (meta == null) {
				meta = xml.CreateElement("metadata");
				xml.SelectSingleNode("/*").AppendChild(meta);
			}

			XmlElement fields = xml.CreateElement("fields");
			fields.Attributes.Append(xml.CreateAttribute("width")).Value = pw.ToString();
			fields.Attributes.Append(xml.CreateAttribute("height")).Value = ph.ToString();
			meta.AppendChild(fields);
			fields.InnerXml = "\n" + flds;
			xml.Save(svgFile);
		}
		void createXml(PdfReader doc) {
			FileInfo fi = new FileInfo(pdfFile.Text);
			string fName = fi.Name.Substring(0, fi.Name.Length - 4);
			StringBuilder buf = new StringBuilder();
			int i;
			var xfa = new XfaForm(doc);
			var som = new XfaForm(doc).TemplateSom;
			buf.Append("<pdf>\n");
			var form = doc.AcroFields;
			var digits = new[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
			string[] strTypes = new string[] {	"unknown", "button", "checkbox",
									"radio", "text", "select", "select", "signiture" };
			for (int pg = 1; pg <= doc.NumberOfPages; pg++) {
				iTextSharp.text.Rectangle pgSz = doc.GetPageSizeWithRotation(pg);
				float ph = pgSz.Height, pw = pgSz.Width;
				prog.Maximum += form.Fields.Count();
				List<Fld> flds = new List<Fld>();
				Dictionary<string, int> fldCnts = new Dictionary<string, int>();
				foreach (var n in form.Fields) {
					AcroFields.Item itm = n.Value;
					if (itm.GetPage(0) != pg) continue;
					//string nm = n.Key.Replace("\\.", "~!~"), db = "", spPfx = "___P" + pg + "___";
					string nm = n.Key, db = "", spPfx = "___P" + pg + "___";
					if ((i = nm.LastIndexOf('.')) >= 0) nm = nm.Substring(i + 1);
					if ((i = nm.LastIndexOf('[')) >= 0) nm = nm.Substring(0, i);
					nm = String.Join("", nm.Replace("~!~", "_")
						.Split(new char[] { '#',' ','_'}, StringSplitOptions.RemoveEmptyEntries));
					// when adding XFLD fields it will start with $pageNum_ -> skip it.
					if (nm.StartsWith(spPfx)) nm = nm.Substring(spPfx.Length);
					if (Char.IsDigit(nm[0])) nm = "F" + nm;
					if (som != null && som.Name2Node != null && som.Name2Node.ContainsKey(n.Key)
					&& som.Name2Node[n.Key].SelectSingleNode("*[@match='global']") == null)
						db = "DB" + pg + "_" + nm;		//local fields get unique data name
					else db = "DB_" + nm;
					var posLst = form.GetFieldPositions(n.Key);
					string cap = itm.GetMerged(0).Get(new PdfName("TU"))?.ToString() ?? "";
					if (cap.StartsWith("Cell") || posLst.Count() > 1)
						cap = "";
					posLst.Select(x => x.position).ForEach(p => {
						//fldCnts[nm]++;
						flds.Add(new Fld {
							name = nm, alt = cap, db = db, 
							top = (pgSz.Height - p.Top), left = p.Left, width = p.Width,
							height = p.Height, type = strTypes[form.GetFieldType(n.Key)]
						});
						//prog.Value++;
						Application.DoEvents();
					});
				}
				flds.GroupBy(x => x.name.TrimEnd(digits)).Where(x => x.Count() > 1)
					.ForEach(x => x.Aggregate(0, (c, n) => {
										n.name = x.Key + (++c);
										n.db = "DB_" + x.Key;
										return c;
									}));
				StringBuilder pb = new StringBuilder();
				flds.OrderBy(f => (int)f.top / 2).ThenBy(f => f.left).ForEach(f => pb.Append(f.ToString()));
				buf.AppendFormat("\t<page id='{0}' svg='{1}_Page{0}.svg' width='{2}' height='{3}'>\n",
					pg, fName, pw, ph).Append(pb).Append("\t</page>\n");
				updateSvgFile(pg, pb.ToString(), (int)pw, (int)ph);
			}
			//buf.Append("</pdf>\n");
			//StreamWriter ssw = new StreamWriter(fi.DirectoryName + @"\" + fName + ".xml", false);
			//ssw.Write(buf);
			//ssw.Close();
		}
		int splitFile() {
			FileInfo fi = new FileInfo(pdfFile.Text);
			if (!fi.Exists) return 0;
			PdfReader rdr = new PdfReader(fi.FullName);
			prog.Maximum = rdr.NumberOfPages;
			prog.Value = 0;
			string baf = string.Format(@"{0}\{1}_Page", fi.DirectoryName,fi.Name.Substring(0,fi.Name.Length-4));
			for (int i = 1; i <= rdr.NumberOfPages; i++) {
				string fn = baf + i + ".pdf";
				if (true || !File.Exists(baf + i + ".svg")) {
					if (!File.Exists(fn)) {
						Document doc = new iTextSharp.text.Document();
						PdfCopy cp = new iTextSharp.text.pdf.PdfCopy(doc, new FileStream(fn, FileMode.Create));
						doc.Open();
						cp.AddPage(cp.GetImportedPage(rdr, i));
						doc.Close();
						cp.Close();
					}
					createSvg(fn);
				}
				File.Delete(fn);
				prog.Value++;
				Application.DoEvents();
			}
			createXml(rdr);
			MessageBox.Show("done");
			return rdr.NumberOfPages;
		}
		private void button2_Click(object sender, EventArgs e) {
			splitFile();

		}
	}
	public static class Utils {
		public static bool IsEmpty(this string s) {
			return s == null || Convert.IsDBNull(s) || s.Length == 0;
		}
		public static IEnumerable<T> ForEach<T>(this IEnumerable<T> arr, Action<T> exp) {
			foreach (T o in arr)
				exp(o);
			return arr;
		}
	}
}
