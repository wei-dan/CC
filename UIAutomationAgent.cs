using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Windows.Automation;

namespace UIAgentLib
{
    /// <summary>
    /// 提供 Windows UI Automation 常用操作的静态方法，所有公开方法均已添加 Description 特性，
    /// 可直接作为 AI Agent 的工具函数使用。
    /// 使用前请确保项目引用了 UIAutomationClient.dll 和 UIAutomationTypes.dll。
    /// </summary>
    public static class UIAgent
    {
        // ========== 私有辅助 ==========

        private static AutomationElement RootElement => AutomationElement.RootElement;

        private static string DescribeElement(AutomationElement element)
        {
            if (element == null)
                return "null";
            var sb = new StringBuilder();
            sb.AppendFormat("Name: {0}", element.Current.Name);
            sb.AppendFormat(", AutomationId: {0}", element.Current.AutomationId);
            sb.AppendFormat(", ControlType: {0}", element.Current.LocalizedControlType);
            var rect = element.Current.BoundingRectangle;
            if (!rect.IsEmpty)
                sb.AppendFormat(", BoundingRect: (X={0}, Y={1}, Width={2}, Height={3})",
                    rect.Left, rect.Top, rect.Width, rect.Height);
            return sb.ToString();
        }

        private static AutomationElement FindElementByAutomationId(string automationId)
        {
            return RootElement.FindFirst(
                TreeScope.Descendants,
                new PropertyCondition(AutomationElement.AutomationIdProperty, automationId));
        }

        private static AutomationElement FindElementByName(string name)
        {
            return RootElement.FindFirst(
                TreeScope.Descendants,
                new PropertyCondition(AutomationElement.NameProperty, name));
        }

        // ========== 公共方法（带有 Description 特性） ==========

        [Description("返回桌面根元素的信息（名称、AutomationId、控件类型等）。")]
        public static string GetDesktopRootInfo()
        {
            var root = RootElement;
            return "DesktopRoot: " + DescribeElement(root);
        }

        [Description("返回当前具有键盘焦点的 UI 元素的信息。")]
        public static string GetFocusedElementInfo()
        {
            var focused = AutomationElement.FocusedElement;
            return "FocusedElement: " + DescribeElement(focused);
        }

        [Description("根据屏幕坐标（x, y）获取该点最顶层 UI 元素的信息。")]
        public static string GetElementFromPoint(
            [Description("X 像素坐标")] int x,
            [Description("Y 像素坐标")] int y)
        {
            var element = AutomationElement.FromPoint(new System.Windows.Point(x, y));
            return $"ElementAt({x},{y}): " + DescribeElement(element);
        }

        [Description("查找 AutomationId 等于指定值的 UI 元素，并返回其信息。")]
        public static string FindByAutomationId(
            [Description("要查找的 AutomationId 值")] string automationId)
        {
            var element = FindElementByAutomationId(automationId);
            return element != null
                ? "Found: " + DescribeElement(element)
                : $"No element found with AutomationId '{automationId}'.";
        }

        [Description("查找 Name 属性等于指定值的 UI 元素，并返回其信息。")]
        public static string FindByName(
            [Description("要查找的 Name 属性值")] string name)
        {
            var element = FindElementByName(name);
            return element != null
                ? "Found: " + DescribeElement(element)
                : $"No element found with Name '{name}'.";
        }

        [Description("通过 AutomationId 找到 UI 元素并对其执行点击（Invoke）操作。")]
        public static string ClickByAutomationId(
            [Description("目标元素的 AutomationId")] string automationId)
        {
            var element = FindElementByAutomationId(automationId);
            if (element == null)
                return $"Click failed: no element with AutomationId '{automationId}'.";

            if (element.TryGetCurrentPattern(InvokePattern.Pattern, out object patternObj))
            {
                ((InvokePattern)patternObj).Invoke();
                return $"Clicked element with AutomationId '{automationId}'.";
            }

            return $"Click failed: element with AutomationId '{automationId}' does not support InvokePattern.";
        }

        [Description("通过 Name 属性找到 UI 元素并对其执行点击（Invoke）操作。")]
        public static string ClickByName(
            [Description("目标元素的 Name 属性")] string name)
        {
            var element = FindElementByName(name);
            if (element == null)
                return $"Click failed: no element with Name '{name}'.";

            if (element.TryGetCurrentPattern(InvokePattern.Pattern, out object patternObj))
            {
                ((InvokePattern)patternObj).Invoke();
                return $"Clicked element with Name '{name}'.";
            }

            return $"Click failed: element with Name '{name}' does not support InvokePattern.";
        }

        [Description("通过 AutomationId 找到 UI 元素并设置其 Value（例如文本框的内容）。")]
        public static string SetValueByAutomationId(
            [Description("目标元素的 AutomationId")] string automationId,
            [Description("要设置的文本值")] string value)
        {
            var element = FindElementByAutomationId(automationId);
            if (element == null)
                return $"SetValue failed: no element with AutomationId '{automationId}'.";

            if (element.TryGetCurrentPattern(ValuePattern.Pattern, out object patternObj))
            {
                ((ValuePattern)patternObj).SetValue(value);
                return $"Set value on element with AutomationId '{automationId}' to '{value}'.";
            }

            return $"SetValue failed: element with AutomationId '{automationId}' does not support ValuePattern.";
        }

        [Description("通过 Name 属性找到 UI 元素并设置其 Value（例如文本框的内容）。")]
        public static string SetValueByName(
            [Description("目标元素的 Name 属性")] string name,
            [Description("要设置的文本值")] string value)
        {
            var element = FindElementByName(name);
            if (element == null)
                return $"SetValue failed: no element with Name '{name}'.";

            if (element.TryGetCurrentPattern(ValuePattern.Pattern, out object patternObj))
            {
                ((ValuePattern)patternObj).SetValue(value);
                return $"Set value on element with Name '{name}' to '{value}'.";
            }

            return $"SetValue failed: element with Name '{name}' does not support ValuePattern.";
        }

        [Description("等待直到出现具有指定 AutomationId 的 UI 元素，超时后返回失败信息。")]
        public static string WaitForElementByAutomationId(
            [Description("目标 AutomationId")] string automationId,
            [Description("超时时间（毫秒）")] int timeoutMilliseconds)
        {
            var deadline = Environment.TickCount + timeoutMilliseconds;
            while (Environment.TickCount < deadline)
            {
                var el = FindElementByAutomationId(automationId);
                if (el != null)
                    return $"Element found within {timeoutMilliseconds}ms: " + DescribeElement(el);

                Thread.Sleep(250);
            }

            return $"WaitForElementByAutomationId: element with AutomationId '{automationId}' not found within {timeoutMilliseconds}ms.";
        }

        [Description("等待直到出现具有指定 Name 属性的 UI 元素，超时后返回失败信息。")]
        public static string WaitForElementByName(
            [Description("目标 Name 属性")] string name,
            [Description("超时时间（毫秒）")] int timeoutMilliseconds)
        {
            var deadline = Environment.TickCount + timeoutMilliseconds;
            while (Environment.TickCount < deadline)
            {
                var el = FindElementByName(name);
                if (el != null)
                    return $"Element found within {timeoutMilliseconds}ms: " + DescribeElement(el);

                Thread.Sleep(250);
            }

            return $"WaitForElementByName: element with Name '{name}' not found within {timeoutMilliseconds}ms.";
        }
    }
}
