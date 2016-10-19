using System;
using System.Collections.Generic;
using System.IO;

namespace Synacor
{
    public class SynacorVM
    {
        public ushort[] vm;
        public int pos = 0;
        public ushort[] registers;
        public Stack<ushort> stack;
        public char outBuffer;

        public SynacorVM(string path)
        {
            vm = LoadVM(path);
            registers = new ushort[8];
            stack = new Stack<ushort>();
        }

        public Opcode RunVM()
        {
            var opcode = getOpcode(pos);
            pos++;
            execOpcode(opcode);
            return opcode;
        }

        public void receiveInput(char c)
        {
            var register = getRegister(pos);
            pos++;
            registers[register] = c;
        }

        public enum Opcode
        {
            HALT = 0,
            SET = 1,
            PUSH = 2,
            POP = 3,
            EQ = 4,
            GT = 5,
            JMP = 6,
            JT = 7,
            JF = 8,
            ADD = 9,
            MULT = 10,
            MOD = 11,
            AND = 12,
            OR = 13,
            NOT = 14,
            RMEM = 15,
            WMEM = 16,
            CALL = 17,
            RET = 18,
            OUT = 19,
            IN = 20,
            NOOP = 21,
            START = 999
        }

        private ushort[] LoadVM(string file)
        {
            int offset = 0;
            //int result = 2;
            var int16Values = new List<ushort>();

            var bytes = File.ReadAllBytes(file);

            while (offset < bytes.Length)
            {
                byte[] buffer = { bytes[offset], bytes[offset + 1] };
                ushort val = BitConverter.ToUInt16(buffer, 0);
                int16Values.Add((ushort)(val % 327668));
                offset += 2;
            }
            return int16Values.ToArray();
        }

        private void execOpcode(Opcode opcode)
        {
            switch (opcode)
            {
                case Opcode.HALT:
                    execHalt();
                    break;
                case Opcode.NOOP:
                    break;
                case Opcode.OUT:
                    execOut();
                    break;
                case Opcode.JMP:
                    execJmp();
                    break;
                case Opcode.JT:
                    execJt();
                    break;
                case Opcode.JF:
                    execJf();
                    break;
                case Opcode.SET:
                    execSet();
                    break;
                case Opcode.ADD:
                    execAdd();
                    break;
                case Opcode.EQ:
                    execEq();
                    break;
                case Opcode.PUSH:
                    execPush();
                    break;
                case Opcode.POP:
                    execPop();
                    break;
                case Opcode.GT:
                    execGt();
                    break;
                case Opcode.AND:
                    execAnd();
                    break;
                case Opcode.OR:
                    execOr();
                    break;
                case Opcode.NOT:
                    execNot();
                    break;
                case Opcode.CALL:
                    execCall();
                    break;
                case Opcode.MULT:
                    execMult();
                    break;
                case Opcode.MOD:
                    execMod();
                    break;
                case Opcode.RMEM:
                    execRmem();
                    break;
                case Opcode.WMEM:
                    execWmem();
                    break;
                case Opcode.RET:
                    execRet();
                    break;
                case Opcode.IN:
                    execIn();
                    break;
                default:
                    throw new NotSupportedException(string.Format("opcode {0} not supported", opcode.ToString()));
            }
        }

        private void execIn()
        {
            //read a character from the terminal and write its ascii code to <a>;
            //it can be assumed that once input starts, it will continue until a newline is encountered;
            //this means that you can safely read whole lines from the keyboard and trust that they will be fully read
            //var key = Console.ReadKey();
            //char c;
            //if (key.Key == ConsoleKey.Enter)
            //    c = '\n';
            //else
            //    c = key.KeyChar;
            //var register = getRegister(pos);
            //pos++;
            //registers[register] = c;
        }

        private void execRet()
        {
            //remove the top element from the stack and jump to it; empty stack = halt
            var element = stack.Pop();
            pos = element;
        }

        private void execWmem()
        {
            //write the value from <b> into memory at address <a> 
            var a = readInt(pos);
            pos++;
            var b = readInt(pos);
            pos++;
            vm[a] = b;
        }

        private void execRmem()
        {
            //read memory at address <b> and write it to <a>
            var register = getRegister(pos);
            pos++;
            var b = readInt(pos);
            pos++;

            registers[register] = vm[b];
        }

        private void execMod()
        {
            //store into <a> the remainder of <b> divided by <c> 
            var register = getRegister(pos);
            pos++;
            var b = readInt(pos);
            pos++;
            var c = readInt(pos);
            pos++;
            registers[register] = (ushort)((b % c) % 32768);
        }

        private void execMult()
        {
            //store into < a > the product of < b > and<c>(modulo 32768)
            var register = getRegister(pos);
            pos++;
            var b = readInt(pos);
            pos++;
            var c = readInt(pos);
            pos++;
            registers[register] = (ushort)((b * c) % 32768);
        }

        private void execCall()
        {
            //write the address of the next instruction to the stack and jump to <a>
            var a = readInt(pos);
            pos++;
            var value = (ushort)(pos % 32768);
            stack.Push(value);
            pos = a;
        }

        private void execNot()
        {
            //stores 15-bit bitwise inverse of <b> in <a> 
            var register = getRegister(pos);
            pos++;
            var b = readInt(pos);
            pos++;

            registers[register] = (ushort)(~b & 0x7FFF);
        }

        private void execOr()
        {
            //stores into < a > the bitwise or of <b> and < c >
            var register = getRegister(pos);
            pos++;
            var b = readInt(pos);
            pos++;
            var c = readInt(pos);
            pos++;
            registers[register] = (ushort)((b | c) % 32768);
        }

        private void execAnd()
        {
            //stores into < a > the bitwise and of <b> and < c >
            var register = getRegister(pos);
            pos++;
            var b = readInt(pos);
            pos++;
            var c = readInt(pos);
            pos++;
            registers[register] = (ushort)((b & c) % 32768);
        }

        private void execGt()
        {
            //set <a> to 1 if <b> is greater than <c>; set it to 0 otherwise                        
            var register = getRegister(pos);
            pos++;
            var b = readInt(pos);
            pos++;
            var c = readInt(pos);
            pos++;
            if (b > c)
                setRegister(register, 1);
            else
                setRegister(register, 0);
        }

        private void execPop()
        {
            // remove the top element from the stack and write it into<a>; empty stack = error

            var a = getRegister(pos);
            if (stack.Count > 0)
            {
                var value = stack.Pop();
                registers[a] = value;
            }
            else
                throw new Exception("Trying to pop empty stack");

            pos++;
        }

        private void execPush()
        {
            //push <a> onto the stack 
            var a = readInt(pos);
            stack.Push(a);
            pos++;
        }

        private int getRegister(int pos)
        {
            var register = vm[pos];
            register = (ushort)((register - 32768) % 32768);
            if (register >= 32768)
                throw new Exception("wtf");
            //register %= 32768;
            if (register < 0 || register > 7)
                throw new ArgumentException("Address invalid");

            return register;
        }

        private void setRegister(int register, ushort value)
        {
            if (register < 0 || register > 7)
                throw new ArgumentException("Address invalid");

            registers[register] = value;
        }

        private void execEq()
        {
            //set <a> to 1 if <b> is equal to <c>; set it to 0 otherwise 
            var registerAddr = pos;
            pos++;

            var b = readInt(pos);
            pos++;

            var c = readInt(pos);
            pos++;

            var a = vm[registerAddr] - 32768;
            if (a < 0 || a > 7)
                throw new ArgumentException(string.Format("Unknown register ({0})", a));

            if (b == c)
                registers[a] = 1;
            else
                registers[a] = 0;
        }

        private void execHalt()
        {
            //stop execution and terminate the program
            //Console.WriteLine("executing HALT, system is shutting down");
            //Console.ReadKey();
            //Environment.Exit(0);
        }

        private void execAdd()
        {
            //assign into <a> the sum of <b> and <c> (modulo 32768) 
            var register = getRegister(pos);
            pos++;
            var b = readInt(pos);
            pos++;
            var c = readInt(pos);
            pos++;

            registers[register] = (ushort)((b + c) % 32768);
        }

        private void execSet()
        {
            //set register < a > to the value of<b>
            var register = getRegister(pos);
            pos++;

            var val = readInt(pos);
            registers[register] = val;

            pos++;
        }

        private void execJf()
        {
            //if < a > is zero, jump to < b >
            ushort a = readInt(pos);
            pos++;
            ushort b = readInt(pos);
            if (a == 0)
                pos = b;
            else
                pos++;
        }

        private void execJt()
        {
            //if <a> is nonzero, jump to <b>
            ushort a = readInt(pos);
            pos++;
            ushort b = readInt(pos);
            if (a != 0)
                pos = b;
            else
                pos++;
        }

        private ushort readInt(int pos)
        {
            if (vm[pos] < 32768)
                return vm[pos];
            else if (vm[pos] < 32776)
                return registers[vm[pos] - 32768];
            else throw new ArgumentException();
        }

        private void execJmp()
        {
            //If a jump was performed, the next operation is instead the exact destination of the jump. 
            pos = readInt(pos);
        }

        private void execOut()
        {
            //write the character represented by ascii code <a> to the terminal 
            char c = (char)(readInt(pos));
            pos++;
            outBuffer = c;
        }

        private Opcode getOpcode(int pos)
        {
            return (Opcode)readInt(pos);
        }        
    }
}
