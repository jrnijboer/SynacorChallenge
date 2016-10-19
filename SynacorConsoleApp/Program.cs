using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Synacor
{
    class Program
    {
        static void Main(string[] args)
        {
            var vm = new SynacorVM(@"vm\challenge.bin");
            var opcode = SynacorVM.Opcode.START;
            Console.WriteLine("Running Synacor VM...");
            while (opcode != SynacorVM.Opcode.HALT)
            {
                opcode = vm.RunVM();
                switch (opcode)
                {
                    case SynacorVM.Opcode.IN:
                        var key = Console.ReadKey();
                        char c;
                        if (key.Key == ConsoleKey.Enter)
                            c = '\n';
                        else
                            c = key.KeyChar;

                        vm.receiveInput(c);
                        break;
                    case SynacorVM.Opcode.OUT:
                        Console.Write(vm.outBuffer);
                        break;
                }
            }
            Console.WriteLine("Shutting down Synacor VM");
        }
    }
}
