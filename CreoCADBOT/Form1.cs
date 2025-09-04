using Newtonsoft.Json;
using OpenAI;
using OpenAI.Chat;
using System.Data;
using System.Text.Json;
using static CreoCADBOT.FunctionCalling;

namespace CreoCADBOT
{
    public partial class Form1 : Form
    {
        private OpenAIClient openClient;
        private ChatClient chatClient;
        private List<ChatMessage> messages;

        public Form1()
        {
            InitializeComponent();
            InitializeBot();
        }

        #region Initialization

        private void InitializeBot()
        {
            openClient = new OpenAIClient("YOUR_API_KEY");
            chatClient = openClient.GetChatClient("gpt-4o");

            string greeting = "You are a Creo CAD bot helping the user. Greet the user and ask how you can help.";
            messages = [new UserChatMessage(greeting)];

            ChatCompletion resp = chatClient.CompleteChat(messages);
            messages.Add(new AssistantChatMessage(resp));

            AddOutGoing(messages.Last().Content[0].Text);
        }

        #endregion

        #region ChatTools

        private static readonly ChatTool GetPartsTool =
            ChatTool.CreateFunctionTool(nameof(GetPartsInCreo), "Get the child parts in Creo");

        private static readonly ChatTool RemovePartTool =
            ChatTool.CreateFunctionTool(
                nameof(RemovePartInCreo),
                "Remove part in Creo",
                functionParameters: BinaryData.FromString("""
                {
                    "type": "object",
                    "properties": {
                        "partName": {
                            "type": "string",
                            "description": "Name of the part"
                        }
                    },
                    "required": ["partName"]
                }
                """));

        private static readonly ChatTool AddPartTool =
            ChatTool.CreateFunctionTool(
                nameof(AddPart),
                "Add a Creo part into assembly",
                functionParameters: BinaryData.FromString("""
                {
                    "type": "object",
                    "properties": {
                        "partName": {
                            "type": "string",
                            "description": "Name of the part"
                        }
                    },
                    "required": ["partName"]
                }
                """));

        private static readonly ChatTool DisplayOnUiTool =
            ChatTool.CreateFunctionTool(
                nameof(AddOutGoingTable),
                "Displays the table on the UI for the user",
                functionParameters: BinaryData.FromString("""
                {
                    "type": "object",
                    "properties": {
                        "tablestring": {
                            "type": "string",
                            "description": "The table to display. Use $ between columns and * between rows."
                        }
                    },
                    "required": ["tablestring"]
                }
                """));

        private static readonly ChatTool HighlightPartTool =
            ChatTool.CreateFunctionTool(
                nameof(HighlightPart),
                "Highlight the part in Creo",
                functionParameters: BinaryData.FromString("""
                {
                    "type": "object",
                    "properties": {
                        "PartName": {
                            "type": "string",
                            "description": "The name of the part"
                        }
                    },
                    "required": ["PartName"]
                }
                """));

        private static readonly ChatTool FetchManufacturingTool =
            ChatTool.CreateFunctionTool(
                nameof(FetchManufacturing),
                "Fetches manufacturing company");

        #endregion

        #region Tool Implementations

        private static string GetPartsInCreo() => allingClass1.GetChildParts();

        private string AddPart(string partName)
        {
            Creo.AssemblePartss(partName);
            return $"{partName} is added in Creo";
        }

        private string RemovePartInCreo(string partName)
        {
            foreach (var part in GlobalVariable.childrens)
            {
                if (part.child.Equals(partName))
                {
                    Creo.DeleteFeature(Convert.ToInt16(part.Id));
                    return $"{partName} is removed in Creo";
                }
            }
            return "Part not found";
        }

        private static string HighlightPart(string partName)
        {
            foreach (var part in GlobalVariable.childrens)
            {
                if (part.child.Equals(partName))
                {
                    Creo.HighlightPart(Convert.ToInt16(part.Id));
                    return $"{partName} is highlighted in Creo";
                }
            }
            return "Part not found";
        }

        private string FetchManufacturing()
        {
            string inputData = @"
Manufacture|Part Name
Jai Liners|PISTON 
Perfect Circle India Limited|PIN_FTC
Riken India|RING_TOP
Goetze India|RING_OIL
India Pistons Limited|RING_BOTTOM";

            DataTable table = new();
            table.Columns.Add("Manufacture", typeof(string));
            table.Columns.Add("Part Name", typeof(string));

            string[] lines = inputData.Trim().Split('\n');
            for (int i = 1; i < lines.Length; i++)
            {
                string[] parts = lines[i].Split('|');
                table.Rows.Add(parts[0].Trim(), parts[1].Trim());
            }

            table.Rows.Add("", "PISTON_PIN");
            return JsonConvert.SerializeObject(table, Formatting.Indented);
        }

        #endregion

        #region Chat Handling

        public string SimpleFunctionCalling(string input)
        {
            bool requireAction = true;
            messages.Add(input);

            ChatCompletionOptions options = new()
            {
                Tools = { GetPartsTool, AddPartTool, RemovePartTool, HighlightPartTool, DisplayOnUiTool, FetchManufacturingTool }
            };

            while (requireAction)
            {
                requireAction = false;
                ChatCompletion resp = chatClient.CompleteChat(messages, options);

                switch (resp.FinishReason)
                {
                    case ChatFinishReason.ToolCalls:
                        messages.Add(new AssistantChatMessage(resp));
                        foreach (ChatToolCall toolCall in resp.ToolCalls)
                        {
                            HandleToolCall(toolCall);
                        }
                        requireAction = true;
                        break;

                    case ChatFinishReason.Stop:
                        messages.Add(new AssistantChatMessage(resp));
                        break;

                    case ChatFinishReason.Length:
                        throw new NotImplementedException();

                    default:
                        throw new NotImplementedException();
                }
            }

            return messages.Last().Content[0].Text;
        }

        private void HandleToolCall(ChatToolCall toolCall)
        {
            switch (toolCall.FunctionName)
            {
                case nameof(GetPartsInCreo):
                    messages.Add(new ToolChatMessage(toolCall.Id, GetPartsInCreo()));
                    break;

                case nameof(HighlightPart):
                    using (JsonDocument argJson = JsonDocument.Parse(toolCall.FunctionArguments))
                    {
                        if (!argJson.RootElement.TryGetProperty("PartName", out JsonElement partName))
                            throw new ArgumentException(nameof(partName), "The PartName argument is required");

                        messages.Add(new ToolChatMessage(toolCall.Id, HighlightPart(partName.GetString())));
                    }
                    break;

                case nameof(AddOutGoingTable):
                    using (JsonDocument argJson = JsonDocument.Parse(toolCall.FunctionArguments))
                    {
                        if (!argJson.RootElement.TryGetProperty("tablestring", out JsonElement tableString))
                            throw new ArgumentException(nameof(tableString), "The table argument is required");

                        AddOutGoingTable(tableString.GetString());
                        messages.Add(new ToolChatMessage(toolCall.Id, "Table is displayed"));
                    }
                    break;

                case nameof(FetchManufacturing):
                    messages.Add(new ToolChatMessage(toolCall.Id, FetchManufacturing()));
                    break;

                case nameof(RemovePartInCreo):
                    using (JsonDocument argJson = JsonDocument.Parse(toolCall.FunctionArguments))
                    {
                        if (!argJson.RootElement.TryGetProperty("partName", out JsonElement partName))
                            throw new ArgumentException(nameof(partName), "The partName argument is required");

                        messages.Add(new ToolChatMessage(toolCall.Id, RemovePartInCreo(partName.GetString())));
                    }
                    break;

                case nameof(AddPart):
                    using (JsonDocument argJson = JsonDocument.Parse(toolCall.FunctionArguments))
                    {
                        if (!argJson.RootElement.TryGetProperty("partName", out JsonElement partName))
                            throw new ArgumentException(nameof(partName), "The partName argument is required");

                        Creo.AssemblePartss(partName.GetString());
                        messages.Add(new ToolChatMessage(toolCall.Id, "Part added"));
                    }
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        #endregion

        #region UI Helpers

        private void AddIncoming(string message)
        {
            var bubble = new incomminglb();
            pnlContainer.Controls.Add(bubble);
            bubble.BringToFront();
            bubble.Dock = DockStyle.Top;
            bubble.Message = message;
        }

        public void AddOutGoing(string message)
        {
            var bubble = new OutGoing();
            pnlContainer.Controls.Add(bubble);
            bubble.BringToFront();
            bubble.Dock = DockStyle.Top;
            bubble.Message = message;
        }

        public void AddOutGoingTable(string tableString)
        {
            DataTable table = new();
            string[] rows = tableString.Split('*');

            foreach (var header in rows[0].Split('$'))
            {
                table.Columns.Add(header);
            }

            for (int i = 1; i < rows.Length; i++)
            {
                table.Rows.Add(rows[i].Split('$'));
            }

            var bubble = new OutGoing();
            pnlContainer.Controls.Add(bubble);
            bubble.BringToFront();
            bubble.Dock = DockStyle.Top;
            bubble.DataGridView = table;
        }

        #endregion

        #region Event Handlers

        private void button1_Click(object sender, EventArgs e)
        {
            HandleUserInput();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            HandleUserInput();
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                HandleUserInput();
        }

        private void HandleUserInput()
        {
            pnlContainer.AutoScroll = true;
            AddIncoming(textBox1.Text);
            string result = SimpleFunctionCalling(textBox1.Text);
            textBox1.Clear();
            AddOutGoing(result);
        }
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
           
        }
        #endregion
    }
}
