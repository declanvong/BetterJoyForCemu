﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterJoyForCemu {
	public static class Config { // stores dynamic configuration, including
		const string PATH = "settings";
		static Dictionary<string, string> variables = new Dictionary<string, string>();

		const int settingsNum = 9; // currently - ProgressiveScan, StartInTray + special buttons

		public static string GetDefaultValue(string s) {
			switch(s) {
				case "ProgressiveScan":
					return "1";
				case "capture":
					return "key_" + ((int)WindowsInput.Events.KeyCode.PrintScreen);
				case "reset_mouse":
					return "joy_" + ((int)Joycon.Button.STICK);
			}
			return "0";
		}

		public static void Init(List<KeyValuePair<string, float[]>> caliData) {
			foreach (string s in new string[] { "ProgressiveScan", "StartInTray", "capture", "home", "sl_l", "sl_r", "sr_l", "sr_r", "reset_mouse" })
				variables[s] = GetDefaultValue(s);

			if (File.Exists(PATH)) {
				int lineNO = 0;
				using (StreamReader file = new StreamReader(PATH)) {
					string line = String.Empty;
					
					while ((line = file.ReadLine()) != null) {
						string[] vs = line.Split();
						try {
							if (lineNO < settingsNum) { // load in basic settings
								variables[vs[0]] = vs[1];
							} else { // load in calibration presets
								caliData.Clear();
								for (int i = 0; i < vs.Length; i++) {
									string[] caliArr = vs[i].Split(',');
									float[] newArr = new float[6];
									for (int j = 1; j < caliArr.Length; j++) {
										newArr[j - 1] = float.Parse(caliArr[j]);
									}
									caliData.Add(new KeyValuePair<string, float[]>(
										caliArr[0],
										newArr
									));
								}
							}
						} catch { }
						lineNO++;
					}

					
				}

				// if old settings
				if (lineNO < settingsNum) {
					File.Delete(PATH);
					Init(caliData);
				}
			} else {
				using (StreamWriter file = new StreamWriter(PATH)) {
					foreach (string k in variables.Keys)
						file.WriteLine(String.Format("{0} {1}", k, variables[k]));
					string caliStr = "";
					for (int i = 0; i < caliData.Count; i++) {
						string space = " ";
						if (i == 0) space = "";
						caliStr += space + caliData[i].Key + "," + String.Join(",", caliData[i].Value);
					}
					file.WriteLine(caliStr);
				}
			}
		}

		public static int IntValue(string key) {
			if (!variables.ContainsKey(key)) {
				return 0;
			}
			return Int32.Parse(variables[key]);
		}

		public static string Value(string key) {
			if (!variables.ContainsKey(key)) {
				return "";
			}
			return variables[key];
		}

		public static bool SetValue(string key, string value) {
			if (!variables.ContainsKey(key))
				return false;
			variables[key] = value;
			return true;
		}

		public static void SaveCaliData(List<KeyValuePair<string, float[]>> caliData) {
			string[] txt = File.ReadAllLines(PATH);
			if (txt.Length < settingsNum + 1) // no custom calibrations yet
				Array.Resize(ref txt, txt.Length + 1);

			string caliStr = "";
			for (int i = 0; i < caliData.Count; i++) {
				string space = " ";
				if (i == 0) space = "";
				caliStr += space + caliData[i].Key + "," + String.Join(",", caliData[i].Value);
			}
			txt[2] = caliStr;
			File.WriteAllLines(PATH, txt);
		}

		public static void Save() {
			string[] txt = File.ReadAllLines(PATH);
			int NO = 0;
			foreach (string k in variables.Keys) {
				txt[NO] = String.Format("{0} {1}", k, variables[k]);
				NO++;
			}
			File.WriteAllLines(PATH, txt);
		}
	}
}