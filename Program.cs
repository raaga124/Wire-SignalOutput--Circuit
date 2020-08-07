using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Q4
{
	//Created properties for the operands
	public class Props
	{
		public bool Computed { set; get; }
		public Operands Operation { set; get; }
		public string Operand1 { set; get; }
		public string Operand2 { set; get; }
		public string Outwire { set; get; }
		public ushort Value { set; get; }
	}
	//to iterate through and save the respective operators
	public enum Operands
	{
		AND,
		OR,
		NOT,
		LSHIFT,
		RSHIFT,
		SAVE
		
	}

	public class Program
	{

		//new list or properties of respective wires to be updated at the end.
		private static List<Props> props = new List<Props>();

		public static void Main()
		{
			//Get input from the text file
			//If you get a system.security exception for reading the input file in the mentioned path. Change your web config as mentioned here:
			// https://forums.asp.net/t/1422162.aspx
			//replace 'fakepath' with the original path for the input file

			
			string input="";
			using (StreamReader file = new StreamReader(@"fakepath"))
			{
				string line;
				while ((line = file.ReadLine()) != null)
				{
					string[] arr = line.Trim().Split(',');
					foreach (string item in arr)
					{
						input +=item +'\n';
					}
				}
			}
			//Load the properties i.e the operands available in the input
			SaveProperties(input);
			//finding the signal values for the wires 
			FindSignalValues();

			//finding the value of the wire 'a' in the list
			var a = props.Where(x => x.Outwire.Equals("a")).FirstOrDefault();
			Console.WriteLine("Signal value of a:{0} ", a.Value);

		}

		//Load properties from input and adding to the list with the respective properties as per the provided input i.e either one wire and signal or //just the signal or two wires with an output wire.
		public static void SaveProperties(String input)
		{
            //read each line for operands
            foreach (string dat in input.Split('\n'))
			{
				if(dat != "")
				{
					string[] datSplit = dat.Split(' ');

					if (dat.Contains("AND"))
					{
						props.Add(LoadTwoWires(datSplit, Operands.AND));
					}
					else if (dat.Contains("OR"))
					{
						props.Add(LoadTwoWires(datSplit, Operands.OR));
					}
					else if (dat.Contains("LSHIFT"))
					{
						props.Add(LoadTwoWires(datSplit, Operands.LSHIFT));
					}
					else if (dat.Contains("RSHIFT"))
					{
						props.Add(LoadTwoWires(datSplit, Operands.RSHIFT));
					}
					else if (dat.Contains("NOT"))
					{
						props.Add(LoadOneWire(datSplit, Operands.NOT));
					}
					//if none of the operators exist we save the values
					else
					{
						props.Add(SaveValues(datSplit, Operands.SAVE));
					}
				}
                else { continue; }
				
			}
		}
		//if the input has two wires, we save the properties accordingly
		public static Props LoadTwoWires(string[] prop, Operands operation)
		{
			return new Props()
			{
				Operand1 = prop[0],
				Operand2 = prop[2],
				Outwire = prop[4],
				Operation = operation
			};
		}
		//if the input has one wire, we save the properties accordingly
		public static Props LoadOneWire(string[] prop, Operands operation)
		{
			return new Props()
			{
				Operand1 = prop[1],
				Outwire = prop[3],
				Operation = operation
			};
		}
		//i input doesn't have the operands, save values in it.
		public static Props SaveValues(string[] prop, Operands operation)
		{
			return new Props()
			{
				Operand1 = prop[0],
				Outwire = prop[2],
				Operation = operation
			};
		}

		public static void FindSignalValues()
		{
			//getting number of properties
			int PropsCount = props.Count();
			int x = 0;
			//for any of the properties whose values are not calculated yet
			while (props.Any(n => !n.Computed))
			{
				var prop = props[x % PropsCount];

				switch (prop.Operation)
				{
					case Operands.AND:
					case Operands.OR:
					case Operands.LSHIFT:
					case Operands.RSHIFT:
						ComputeTwoWireSignals(prop, prop.Operation);
						break;

					case Operands.NOT:
						ComputeNotOperand(prop);
						break;

					case Operands.SAVE:
						ComputeSave(prop);
						break;

					default:
						throw new Exception("not valid operator found");
				}

				x++;
			}
		}

		public static void ComputeTwoWireSignals(Props prop, Operands operation)
		{
			//this is the computed value after applying the operands.
			ushort final = 0;

			//converting the string representation of a number to its 16-bit unsigned integer equivalent
			if (ushort.TryParse(prop.Operand1, out ushort operand1) && ushort.TryParse(prop.Operand2, out ushort operand2))
			{

				switch (operation)
				{
					//Compute Let shift operand
					case Operands.LSHIFT:
						final = (ushort)(operand1 << operand2);
						break;

					//Compute right shift operand
					case Operands.RSHIFT:
						final = (ushort)(operand1 >> operand2);
						break;

					//Compute And operand
					case Operands.AND:
						final = (ushort)(operand1 & operand2);
						break;

					//Compute OR operand
					case Operands.OR:
						final = (ushort)(operand1 | operand2);
						break;

				}

				SaveOutWire(prop.Outwire, (ushort)final);
				prop.Value = final;
				prop.Computed = true;
			}
		}


		public static void SaveOutWire(string outWire, ushort signal)
		{
			//update the values in the list with the computed signals for the respective wires
			foreach (var j in props.Where(x => x.Operand1 != null && x.Operand1.Equals(outWire)))
			{
				j.Operand1 = signal.ToString();
			}

			foreach (var j in props.Where(x => x.Operand2 != null && x.Operand2.Equals(outWire)))
			{
				j.Operand2 = signal.ToString();
			}
		}

		public static void ComputeNotOperand(Props prop)
		{
			if (ushort.TryParse(prop.Operand1, out ushort operand1))
			{
				ushort result = (ushort)(~operand1);
				SaveOutWire(prop.Outwire, result);
				prop.Value = result;
				prop.Computed = true;
			}
		}
		//save signal value
		public static void ComputeSave(Props prop)
		{
			if (ushort.TryParse(prop.Operand1, out ushort operand1))
			{
				SaveOutWire(prop.Outwire, operand1);
				prop.Value = operand1;
				prop.Computed = true;
			}
		}


	}


}
