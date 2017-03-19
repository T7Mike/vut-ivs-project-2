/**************************************************************
 * Team:      Disassembler
 *
 * Authors:   Vojtěch Hertl <xhertl04@stud.fit.vutbr.cz>
 *            Dominik Harmim <xharmi00@stud.fit.vutbr.cz>
 *            Timotej Halás <xhalas10@stud.fit.vutbr.cz>
 *            Matej Havlas <xhavla06@stud.fit.vutbr.cz>
 **************************************************************/

using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Disassembler.Calculator
{
	/// <summary>
	///     Output result processor.
	/// </summary>
	public class OutputProcessor
	{
		// TextAns TextBox properties
		private const int TextAnsMaxLength = 20;
		private const int TextAnsBasicFontLimit = 12;
		private const double TextAnsBasicFontSize = 44.0;
		private const double TextAnsValueOfFontSizeReduction = 5.6;

		// culture
		private const string Culture = "cs-CZ";

		// TextAns TextBox
		private readonly TextBox textAns;

		// TextLog TextBox
		private readonly TextBox textLog;

		// indicates whether the result is displayed
		public bool IsAnswer { get; set; } = true;

		/// <summary>
		///     OutputProcessor construct.
		/// </summary>
		/// <param name="textAns">TextAns TextBox.</param>
		/// <param name="textLog">TextLog TextBox.</param>
		public OutputProcessor(TextBox textAns, TextBox textLog)
		{
			this.textAns = textAns;
			this.textLog = textLog;

			// + 2 becouse '-' and ',' char
			this.textAns.MaxLength = TextAnsMaxLength + 2;
			this.textAns.FontSize = TextAnsBasicFontSize;
		}

		// indicates whether the result is displayed in log (eg. factorial)
		public bool ResultInLog { get; set; }

		/// <summary>
		///     Print number to TextAns TextBox in proper format.
		/// </summary>
		/// <param name="number">Pressed number in string format.</param>
		/// <returns>True, if number was printed, false otherwise.</returns>
		public bool PrintNumber(string number)
		{
			// if result is in log, then clear log
			if (this.ResultInLog)
			{
				this.ClearLog();
				MathProcessor.ClearResult();
			}
			this.ResultInLog = false;

			// max length was reached
			if (this.textAns.Text.Length > TextAnsMaxLength)
			{
				return false;
			}

			// basic font limit was reached -> reduce font size
			if (this.textAns.Text.Length > TextAnsBasicFontLimit)
			{
				// remove non numeric chars
				string textAnsText = Utils.RemoveSpaces(Utils.RemoveChars(this.textAns.Text, new[] {',', '-'}));
				// reduce font size only once per two numbers
				if (textAnsText.Length % 2 == 0)
				{
					this.textAns.FontSize -= TextAnsValueOfFontSizeReduction;
				}
			}
			else
			{
				this.textAns.FontSize = TextAnsBasicFontSize;
			}

			if (this.textAns.Text == "0" || this.IsAnswer)
			{
				this.textAns.Text = number;
			}
			else
			{
				this.textAns.Text += number;
			}

			this.textAns.Text = this.FormatNumericValue(this.textAns.Text);
			this.IsAnswer = false;

			return true;
		}

		/// <summary>
		///     Print decimal separator (comma).
		/// </summary>
		public void PrintComma()
		{
			if (this.IsAnswer)
			{
				this.textAns.Text = "0,";
			}
			else if (!this.textAns.Text.Contains(","))
			{
				this.textAns.Text += ",";
			}

			this.IsAnswer = false;
		}

		/// <summary>
		///     Invert Ans value.
		/// </summary>
		public void InvertAns()
		{
			if (this.IsAnswer || this.IsAnsClear())
			{
				return;
			}

			if (this.textAns.Text.Contains("-"))
			{
				this.textAns.Text = this.textAns.Text.Substring(1, this.textAns.Text.Length - 1);
			}
			else
			{
				this.textAns.Text = "-" + this.textAns.Text;
			}
		}

		/// <summary>
		///     Print answer.
		/// </summary>
		/// <param name="answer">Answer in double format.</param>
		public void PrintAns(double answer)
		{
			this.textAns.Text = this.FormatNumericValue(answer);
			this.textAns.FontSize = TextAnsBasicFontSize;
			if (!this.ResultInLog)
			{
				this.IsAnswer = true;
			}
		}

		/// <summary>
		///     Print text log.
		/// </summary>
		/// <param name="operation">Operation in string format.</param>
		public void PrintLog(string operation)
		{
			// change last operator
			if (this.textLog.Text.Length != 0 && this.IsAnswer && !this.ResultInLog)
			{
				// remove last whitespace
				this.textLog.Text = this.textLog.Text.Remove(this.textLog.Text.Length - 1);
				// split text by whitespaces to array
				string[] textArray = this.textLog.Text.Split(' ');
				// remove operator (last array item)
				textArray = textArray.Take(textArray.Length - 1).ToArray();
				// join array to string
				this.textLog.Text = string.Join(" ", textArray);
				// add operator
				this.textLog.Text += " " + operation + " ";
			}

			if (!this.IsAnswer)
			{
				this.textLog.Text += (this.ResultInLog ? "" : this.textAns.Text) + " " + operation + " ";
			}

			this.textLog.CaretIndex = this.textLog.Text.Length;
			Rect rect = this.textLog.GetRectFromCharacterIndex(this.textLog.CaretIndex);
			this.textLog.ScrollToHorizontalOffset(rect.Right);
			this.ResultInLog = false;
		}

		/// <summary>
		///     Clear log.
		/// </summary>
		public void ClearLog()
		{
			this.textLog.Text = "";
		}

		/// <summary>
		///     Clear answer.
		/// </summary>
		public void ClearAns()
		{
			this.textAns.Text = "0";
			this.IsAnswer = false;
			this.textAns.FontSize = TextAnsBasicFontSize;
		}

		/// <summary>
		///     Is answer clear?
		/// </summary>
		/// <returns>True, if answer is clear, false otherwise.</returns>
		private bool IsAnsClear()
		{
			return this.textAns.Text == "0";
		}

		/// <summary>
		///     Remove last number.
		/// </summary>
		public void Backspace()
		{
			if (this.IsAnswer || this.IsAnsClear())
			{
				return;
			}

			if (this.textAns.Text.Length == 1 || this.textAns.Text.Length == 2 && this.textAns.Text.Contains("-"))
			{
				this.ClearAns();
				return;
			}

			bool containsComma = this.textAns.Text.Contains(",");
			int removeLength = 1;
			// if penultimate char is whitespace, then remove it too
			if (string.IsNullOrWhiteSpace(this.textAns.Text.Substring(this.textAns.Text.Length - 2, 1)))
			{
				removeLength++;
			}
			this.textAns.Text = this.textAns.Text.Remove(this.textAns.Text.Length - removeLength);

			// basic font limit is reached -> increase font size
			if (this.textAns.Text.Length > TextAnsBasicFontLimit && (!containsComma || this.textAns.Text.Contains(",")))
			{
				// remove non numeric chars
				string textAnsText = Utils.RemoveSpaces(Utils.RemoveChars(this.textAns.Text, new[] {',', '-'}));
				// increase font size only once per two numbers
				if (textAnsText.Length % 2 == 0)
				{
					this.textAns.FontSize += TextAnsValueOfFontSizeReduction;
				}
			}

			this.textAns.Text = this.FormatNumericValue(this.textAns.Text);
		}

		/// <summary>
		///     Format numeric value in string to czech number format.
		/// </summary>
		/// <param name="value">Numeric value in string format to be formatted.</param>
		/// <returns>Formated numeric value in string format.</returns>
		private string FormatNumericValue(string value)
		{
			long numericValue;
			string formatedValue;
			CultureInfo cultureInfo = new CultureInfo(Culture);
			value = Utils.RemoveSpaces(value);
			int indexOfDecPoint = value.IndexOf(",", StringComparison.Ordinal);
			if (indexOfDecPoint == -1) // value isn't decimal
			{
				// format number
				long.TryParse(value, out numericValue);
				formatedValue = numericValue.ToString("N", cultureInfo);
				// remove decimal places
				formatedValue = formatedValue.Remove(formatedValue.Length - 3);
			}
			else // value is decimal
			{
				// format non decimal part of number
				string integerPart = value.Substring(0, indexOfDecPoint);
				long.TryParse(integerPart, out numericValue);
				formatedValue = numericValue.ToString("N", cultureInfo);
				// remove decimal places
				formatedValue = formatedValue.Remove(formatedValue.Length - 3);

				// format decimal part of number
				string decimalPart = value.Substring(indexOfDecPoint, value.Length - integerPart.Length);
				formatedValue += decimalPart;
			}

			return formatedValue;
		}

		/// <summary>
		///     Format numeric value in double format to czech number format.
		/// </summary>
		/// <param name="value">Numeric value in double format to be formatted.</param>
		/// <returns>Formated numeric value in string format.</returns>
		private string FormatNumericValue(double value)
		{
			CultureInfo cultureInfo = new CultureInfo(Culture);
			string formatedValue = value.ToString("R", cultureInfo);

			// remove non numeric chars except spaces
			int formatedValueLenght = Utils.RemoveChars(formatedValue, new[] {',', '-'}).Length;

			formatedValue = formatedValueLenght > TextAnsMaxLength
				? value.ToString("g2", cultureInfo)
				: this.FormatNumericValue(formatedValue);

			return formatedValue;
		}
	}
}
