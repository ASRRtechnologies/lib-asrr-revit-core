using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Media;

namespace ASRR.Revit.Core.UI.RibbonPanelItemCreator
{
    public class StackedButtonsRibbonPanelItemCreator : IRibbonPanelItemCreator
    {
        private readonly List<StackedButtonDefinition> _buttons;

        public StackedButtonsRibbonPanelItemCreator(List<StackedButtonDefinition> buttons)
        {
            if (buttons == null || buttons.Count < 2 || buttons.Count > 3)
                throw new ArgumentException("Stacked buttons require 2 or 3 button definitions");
            _buttons = buttons;
        }

        public void Create(RibbonPanel panel)
        {
            var buttonDataList = new List<PushButtonData>();

            foreach (var btn in _buttons)
            {
                var commandAssembly = Assembly.GetAssembly(btn.CommandType);
                var uri = new UriBuilder(commandAssembly.CodeBase);
                var path = Uri.UnescapeDataString(uri.Path);

                var data = new PushButtonData(btn.Title, btn.Title, path, btn.CommandType.FullName);
                if (btn.Image != null)
                    data.Image = btn.Image;
                buttonDataList.Add(data);
            }

            if (buttonDataList.Count == 2)
                panel.AddStackedItems(buttonDataList[0], buttonDataList[1]);
            else
                panel.AddStackedItems(buttonDataList[0], buttonDataList[1], buttonDataList[2]);
        }
    }

    public class StackedButtonDefinition
    {
        public string Title { get; set; }
        public Type CommandType { get; set; }
        public ImageSource Image { get; set; }

        public StackedButtonDefinition(string title, Type commandType, ImageSource image = null)
        {
            Title = title;
            CommandType = commandType;
            Image = image;
        }
    }
}
