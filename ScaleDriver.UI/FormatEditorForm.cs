using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using ScaleDriver.Core;

namespace ScaleDriver.UI
{
    /// <summary>
    /// Form for editing and testing regex patterns for weight frame parsing
    /// </summary>
    public partial class FormatEditorForm : Form
    {
        private TextBox txtName;
        private TextBox txtPattern;
        private TextBox txtWeightGroup;
        private TextBox txtUnitGroup;
        private TextBox txtStabilityGroup;
        private TextBox txtDescription;
        private TextBox txtTestData;
        private TextBox txtResult;
        private Button btnTest;
        private Button btnSave;
        private Button btnCancel;
        private Label lblStatus;

        public RegexParser Parser { get; private set; }

        public FormatEditorForm()
        {
            InitializeComponent();
        }

        public FormatEditorForm(RegexParser existingParser) : this()
        {
            if (existingParser != null)
            {
                txtName.Text = existingParser.Name;
                txtPattern.Text = existingParser.Pattern;
                txtDescription.Text = existingParser.Description;
                // Note: Group names are private, so we use defaults
                txtWeightGroup.Text = "weight";
                txtUnitGroup.Text = "unit";
                txtStabilityGroup.Text = "stable";
            }
        }

        private void InitializeComponent()
        {
            this.Text = "Regex Format Editor";
            this.Size = new Size(800, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            int labelWidth = 120;
            int controlLeft = labelWidth + 20;
            int controlWidth = 620;
            int rowHeight = 30;
            int currentY = 20;

            // Name
            var lblName = new Label
            {
                Text = "Parser Name:",
                Location = new Point(10, currentY + 3),
                Size = new Size(labelWidth, 20)
            };
            txtName = new TextBox
            {
                Location = new Point(controlLeft, currentY),
                Size = new Size(controlWidth, 25)
            };
            this.Controls.Add(lblName);
            this.Controls.Add(txtName);
            currentY += rowHeight;

            // Description
            var lblDescription = new Label
            {
                Text = "Description:",
                Location = new Point(10, currentY + 3),
                Size = new Size(labelWidth, 20)
            };
            txtDescription = new TextBox
            {
                Location = new Point(controlLeft, currentY),
                Size = new Size(controlWidth, 25)
            };
            this.Controls.Add(lblDescription);
            this.Controls.Add(txtDescription);
            currentY += rowHeight;

            // Pattern
            var lblPattern = new Label
            {
                Text = "Regex Pattern:",
                Location = new Point(10, currentY + 3),
                Size = new Size(labelWidth, 20)
            };
            txtPattern = new TextBox
            {
                Location = new Point(controlLeft, currentY),
                Size = new Size(controlWidth, 60),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical
            };
            this.Controls.Add(lblPattern);
            this.Controls.Add(txtPattern);
            currentY += 70;

            // Weight Group
            var lblWeightGroup = new Label
            {
                Text = "Weight Group:",
                Location = new Point(10, currentY + 3),
                Size = new Size(labelWidth, 20)
            };
            txtWeightGroup = new TextBox
            {
                Location = new Point(controlLeft, currentY),
                Size = new Size(200, 25),
                Text = "weight"
            };
            this.Controls.Add(lblWeightGroup);
            this.Controls.Add(txtWeightGroup);
            currentY += rowHeight;

            // Unit Group
            var lblUnitGroup = new Label
            {
                Text = "Unit Group:",
                Location = new Point(10, currentY + 3),
                Size = new Size(labelWidth, 20)
            };
            txtUnitGroup = new TextBox
            {
                Location = new Point(controlLeft, currentY),
                Size = new Size(200, 25),
                Text = "unit"
            };
            this.Controls.Add(lblUnitGroup);
            this.Controls.Add(txtUnitGroup);
            currentY += rowHeight;

            // Stability Group
            var lblStabilityGroup = new Label
            {
                Text = "Stability Group:",
                Location = new Point(10, currentY + 3),
                Size = new Size(labelWidth, 20)
            };
            txtStabilityGroup = new TextBox
            {
                Location = new Point(controlLeft, currentY),
                Size = new Size(200, 25),
                Text = "stable"
            };
            this.Controls.Add(lblStabilityGroup);
            this.Controls.Add(txtStabilityGroup);
            currentY += rowHeight + 10;

            // Test Data
            var lblTestData = new Label
            {
                Text = "Test Data:",
                Location = new Point(10, currentY + 3),
                Size = new Size(labelWidth, 20)
            };
            txtTestData = new TextBox
            {
                Location = new Point(controlLeft, currentY),
                Size = new Size(controlWidth, 80),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Text = "ST,GS,+00012.50,kg"
            };
            this.Controls.Add(lblTestData);
            this.Controls.Add(txtTestData);
            currentY += 90;

            // Test Button
            btnTest = new Button
            {
                Text = "Test Pattern",
                Location = new Point(controlLeft, currentY),
                Size = new Size(120, 30)
            };
            btnTest.Click += BtnTest_Click;
            this.Controls.Add(btnTest);
            currentY += 40;

            // Result
            var lblResult = new Label
            {
                Text = "Result:",
                Location = new Point(10, currentY + 3),
                Size = new Size(labelWidth, 20)
            };
            txtResult = new TextBox
            {
                Location = new Point(controlLeft, currentY),
                Size = new Size(controlWidth, 100),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                ReadOnly = true
            };
            this.Controls.Add(lblResult);
            this.Controls.Add(txtResult);
            currentY += 110;

            // Status Label
            lblStatus = new Label
            {
                Text = "",
                Location = new Point(10, currentY),
                Size = new Size(controlWidth + controlLeft - 10, 20),
                ForeColor = Color.Green
            };
            this.Controls.Add(lblStatus);
            currentY += 30;

            // Buttons
            btnSave = new Button
            {
                Text = "Save",
                Location = new Point(controlLeft + controlWidth - 180, currentY),
                Size = new Size(80, 30),
                DialogResult = DialogResult.OK
            };
            btnSave.Click += BtnSave_Click;

            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(controlLeft + controlWidth - 90, currentY),
                Size = new Size(80, 30),
                DialogResult = DialogResult.Cancel
            };

            this.Controls.Add(btnSave);
            this.Controls.Add(btnCancel);
            this.AcceptButton = btnSave;
            this.CancelButton = btnCancel;
        }

        private void BtnTest_Click(object? sender, EventArgs e)
        {
            try
            {
                var name = txtName.Text.Trim();
                var pattern = txtPattern.Text.Trim();
                var weightGroup = txtWeightGroup.Text.Trim();
                var unitGroup = txtUnitGroup.Text.Trim();
                var stabilityGroup = txtStabilityGroup.Text.Trim();
                var testData = txtTestData.Text.Trim();

                if (string.IsNullOrEmpty(pattern))
                {
                    MessageBox.Show("Please enter a regex pattern.", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (string.IsNullOrEmpty(testData))
                {
                    MessageBox.Show("Please enter test data.", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var parser = new RegexParser(
                    string.IsNullOrEmpty(name) ? "TestParser" : name,
                    pattern,
                    string.IsNullOrEmpty(weightGroup) ? "weight" : weightGroup,
                    string.IsNullOrEmpty(unitGroup) ? "unit" : unitGroup,
                    string.IsNullOrEmpty(stabilityGroup) ? "stable" : stabilityGroup
                );

                var canParse = parser.CanParse(testData);
                var confidence = parser.GetConfidenceScore(testData);
                var result = parser.Parse(testData);

                txtResult.Text = $"Can Parse: {canParse}\r\n";
                txtResult.AppendText($"Confidence Score: {confidence}\r\n");
                txtResult.AppendText($"Parse Success: {result.Success}\r\n");
                
                if (result.Success)
                {
                    txtResult.AppendText($"Weight: {result.Weight}\r\n");
                    txtResult.AppendText($"Unit: {result.Unit}\r\n");
                    txtResult.AppendText($"Stable: {result.IsStable}\r\n");
                }
                else
                {
                    txtResult.AppendText($"Error: {result.ErrorMessage}\r\n");
                }

                lblStatus.Text = "Test completed successfully";
                lblStatus.ForeColor = Color.Green;
            }
            catch (Exception ex)
            {
                txtResult.Text = $"Error: {ex.Message}";
                lblStatus.Text = "Test failed";
                lblStatus.ForeColor = Color.Red;
            }
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            try
            {
                var name = txtName.Text.Trim();
                var pattern = txtPattern.Text.Trim();
                var weightGroup = txtWeightGroup.Text.Trim();
                var unitGroup = txtUnitGroup.Text.Trim();
                var stabilityGroup = txtStabilityGroup.Text.Trim();
                var description = txtDescription.Text.Trim();

                if (string.IsNullOrEmpty(name))
                {
                    MessageBox.Show("Please enter a parser name.", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.DialogResult = DialogResult.None;
                    return;
                }

                if (string.IsNullOrEmpty(pattern))
                {
                    MessageBox.Show("Please enter a regex pattern.", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.DialogResult = DialogResult.None;
                    return;
                }

                Parser = new RegexParser(
                    name,
                    pattern,
                    string.IsNullOrEmpty(weightGroup) ? "weight" : weightGroup,
                    string.IsNullOrEmpty(unitGroup) ? "unit" : unitGroup,
                    string.IsNullOrEmpty(stabilityGroup) ? "stable" : stabilityGroup,
                    description
                );

                lblStatus.Text = "Parser saved successfully";
                lblStatus.ForeColor = Color.Green;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating parser: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.DialogResult = DialogResult.None;
            }
        }
    }
}
