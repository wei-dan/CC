using System;

while (true)
{
    Console.Write("You: ");
    string? input = Console.ReadLine();

    // 当输入流结束（EOF）时退出循环
    if (input is null)
    {
        break;
    }

    Console.WriteLine("Bot: hello");
}
