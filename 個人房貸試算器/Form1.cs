using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;

namespace 個人房貸試算器
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            // 預設值
            textBox1.Text = "10000000";   // 房屋總價
            textBox2.Text = "20";         // 自備款比例
            textBox3.Text = "2.15";       // 年利率
            textBox4.Text = "30";         // 貸款年限
            textBox5.Text = "0";          // 寬限期

            radio_rdo.Checked = true;  // 預設用比例(%)

            ClearOutput();
        }

        private void run_btn_Click(object sender, EventArgs e)
        {
            try
            {
                double housePrice, downInput, annualRate;
                int loanYears, graceYears;

                // ===== 輸入驗證 =====
                if (!double.TryParse(textBox1.Text.Trim(), out housePrice) || housePrice <= 0)
                {
                    MessageBox.Show("請輸入正確的房屋總價。");
                    return;
                }

                if (!double.TryParse(textBox2.Text.Trim(), out downInput) || downInput < 0)
                {
                    MessageBox.Show("請輸入正確的自備款比例或金額。");
                    return;
                }

                if (!double.TryParse(textBox3.Text.Trim(), out annualRate) || annualRate < 0)
                {
                    MessageBox.Show("請輸入正確的貸款利率。");
                    return;
                }

                if (!int.TryParse(textBox4.Text.Trim(), out loanYears) || loanYears <= 0)
                {
                    MessageBox.Show("請輸入正確的貸款年限。");
                    return;
                }

                if (!int.TryParse(textBox5.Text.Trim(), out graceYears) || graceYears < 0)
                {
                    MessageBox.Show("請輸入正確的寬限期。");
                    return;
                }

                if (graceYears > loanYears)
                {
                    MessageBox.Show("寬限期不能大於貸款年限。");
                    return;
                }

                // ===== 自備款計算 =====
                double downPayment;

                if (radio_rdo.Checked) // 比例(%)
                {
                    if (downInput > 100)
                    {
                        MessageBox.Show("自備款比例不可大於 100%。");
                        return;
                    }
                    downPayment = housePrice * (downInput / 100.0);
                }
                else if (money_rdo.Checked) // 金額(元)
                {
                    if (downInput > housePrice)
                    {
                        MessageBox.Show("自備款金額不可大於房屋總價。");
                        return;
                    }
                    downPayment = downInput;
                }
                else
                {
                    MessageBox.Show("請選擇自備款輸入方式。");
                    return;
                }

                // ===== 基本參數 =====
                double loanAmount = housePrice - downPayment;
                double monthlyRate = annualRate / 100.0 / 12.0;
                int totalMonths = loanYears * 12;
                int graceMonths = graceYears * 12;
                int repayMonths = totalMonths - graceMonths;

                double monthlyPayment = 0.0;
                double firstInterest = 0.0;
                double firstPrincipal = 0.0;
                double totalInterest = 0.0;
                double totalPayment = 0.0;

                if (loanAmount <= 0)
                {
                    MessageBox.Show("貸款總金額需大於 0。");
                    return;
                }

                // ===== 計算 =====
                if (monthlyRate == 0) // 0利率特別處理
                {
                    if (graceMonths > 0)
                    {
                        monthlyPayment = loanAmount / repayMonths;
                        firstInterest = 0;
                        firstPrincipal = 0; // 寬限期首期只繳利息，0利率時就是0
                        totalInterest = 0;
                        totalPayment = loanAmount;
                    }
                    else
                    {
                        monthlyPayment = loanAmount / totalMonths;
                        firstInterest = 0;
                        firstPrincipal = monthlyPayment;
                        totalInterest = 0;
                        totalPayment = loanAmount;
                    }
                }
                else
                {
                    if (graceMonths > 0)
                    {
                        // 寬限期：只繳利息
                        double graceMonthlyInterest = loanAmount * monthlyRate;

                        // 寬限期後，本金不變，再用剩下月數攤還
                        monthlyPayment = loanAmount * monthlyRate *
                                         Math.Pow(1 + monthlyRate, repayMonths) /
                                         (Math.Pow(1 + monthlyRate, repayMonths) - 1);

                        // 首期是在寬限期內
                        firstInterest = graceMonthlyInterest;
                        firstPrincipal = 0;

                        totalInterest = graceMonthlyInterest * graceMonths
                                      + (monthlyPayment * repayMonths - loanAmount);

                        totalPayment = loanAmount + totalInterest;
                    }
                    else
                    {
                        // 一般本息平均攤還
                        monthlyPayment = loanAmount * monthlyRate *
                                         Math.Pow(1 + monthlyRate, totalMonths) /
                                         (Math.Pow(1 + monthlyRate, totalMonths) - 1);

                        firstInterest = loanAmount * monthlyRate;
                        firstPrincipal = monthlyPayment - firstInterest;

                        totalInterest = monthlyPayment * totalMonths - loanAmount;
                        totalPayment = loanAmount + totalInterest;
                    }
                }

                // ===== 輸出 =====
                label12.Text = FormatMoney(loanAmount);       // 貸款總金額
                label13.Text = FormatMoney(monthlyPayment);   // 每月應繳金額
                label14.Text = FormatMoney(firstInterest);    // 首期利息
                label15.Text = FormatMoney(firstPrincipal);   // 首期本金
                label16.Text = FormatMoney(totalInterest);    // 總利息支出
                label17.Text = FormatMoney(totalPayment);     // 總還款金額
            }
            catch (Exception ex)
            {
                MessageBox.Show("發生錯誤：" + ex.Message);
            }
        }


        private void clean_btn_Click(object sender, EventArgs e)
        {
            textBox1.Clear();
            textBox2.Clear();
            textBox3.Clear();
            textBox4.Clear();
            textBox5.Clear();

            radio_rdo.Checked = true;

            ClearOutput();
            textBox1.Focus();
        }

        private void ClearOutput()
        {
            label12.Text = "";
            label13.Text = "";
            label14.Text = "";
            label15.Text = "";
            label16.Text = "";
            label17.Text = "";
        }

        private string FormatMoney(double value)
        {
            return value.ToString("N2", CultureInfo.InvariantCulture);
        }

    }
}
