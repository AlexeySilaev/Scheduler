using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Reminder.Resources.Cipher
{
    public class BitwiseScrambler
    {
        private const ushort POLYNOMIAL = 0xB400; // x^16 + x^14 + x^13 + x^11 + 1
        private readonly ushort _seed;
        ushort _state;

        public BitwiseScrambler()
        {
            AssemblyName assemblyName = Assembly.GetExecutingAssembly().GetName();
            byte[] keyToken = assemblyName.GetPublicKeyToken();
            _seed = BitConverter.ToUInt16(keyToken, 5);
        }

        public void Scramble(string sourcePath, string targetPath)
        {
            _state = _seed;
            using (BinaryReader reader = new BinaryReader(File.OpenRead(sourcePath)))
            using (BinaryWriter writer = new BinaryWriter(File.Create(targetPath)))
            {
                while (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    byte[] read = reader.ReadBytes(4 * 1024);
                    byte[] xorResult = ScrambleBuffer(read);
                    writer.Write(xorResult);
                }
            }
        }

        private byte[] ScrambleBuffer(byte[] buffer)
        {
            var result = new byte[buffer.Length];

            for (int i = 0; i < buffer.Length; i++)
            {
                result[i] = ScrambleByte(buffer[i]);
            }
            return result;
        }

        private byte ScrambleByte(byte b)
        {
            byte result = 0;
            for (int bitPos = 7; bitPos >= 0; bitPos--)
            {
                // Выходной бит — старший ДО сдвига (бит, который вылетает из регистра)
                byte outputBit = (byte)((_state >> 15) & 1);

                // Сдвиг влево
                _state = (ushort)(_state << 1);

                // Если вылетевший бит = 1, применяем полином
                if (outputBit == 1)
                    _state ^= POLYNOMIAL;

                // XOR исходного бита с выходным битом LFSR
                byte originalBit = (byte)((b >> bitPos) & 1);
                byte xorBit = (byte)(originalBit ^ outputBit);
                result |= (byte)(xorBit << bitPos);
            }
            return result;
        }
    }
}
