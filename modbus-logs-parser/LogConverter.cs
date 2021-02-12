using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;

namespace test_app_for_techart
{
    public class LogConverter
    {
        private enum LogField
        {
            Id,
            Time,
            AppName,
            Command,
            Source,
            Result,
            Data
        }

        // Возвращает следующее значение в списке или defaultValue
        private int GetNextValueAfterOrReturnValue(List<int> values, int target, int defaultValue)
        {
            for (int i = 0; i < values.Count; ++i)
                if ((values[i] == target) && (i + 1 < values.Count))
                    return values[i + 1];
            return defaultValue;
        }

        // Возвращает список ID для следующего вывода
        private List<int> GetNextID(int current, List<int> ids, List<LogEntry> log)
        {
            List<int> mas = new List<int>
            {
                current
            };
            LogEntry reference = log[current];

            for (int i = ids.IndexOf(current) + 1; i < ids.Count; ++i)
            {
                if (log[ids[i]].App == reference.App &&
                    log[ids[i]].Direction == reference.Direction &&
                    log[ids[i]].Result == reference.Result &&
                    log[ids[i]].Serial == reference.Serial)
                {
                    mas.Add(ids[i]);
                }
                else
                {
                    break;
                }
            }
            return mas;
        }

        private void Process(List<int> idsToPrint, List<LogEntry> entries, Action<LogEntry> output)
        {
            if (entries.Count == 0 ||
                idsToPrint.Count == 0 ||
                idsToPrint.Count > entries.Count)
                return;

            // Если запись всего одна, то сразу выводим её
            if (idsToPrint.Count == 1)
            {
                output(entries[idsToPrint.First()]);
                return;
            }

            int i = idsToPrint.First();
            while (true)
            {
                List<int> ids = GetNextID(i, idsToPrint, entries);
                if (ids.Count == 1)
                {
                    output(entries[ids.Last()]);
                }
                else
                {
                    // Создаем временную запись
                    LogEntry union = new LogEntry
                    {
                        Id = -1,
                        App = entries[ids.First()].App,
                        Serial = entries[ids.First()].Serial,
                        Result = entries[ids.First()].Result,
                        Direction = entries[ids.First()].Direction,
                        DataLength = 0
                    };

                    //Считаем общий размер массива данных
                    foreach (var id in ids)
                        union.DataLength += entries[id].DataLength;

                    //Создаем новый массив
                    union.Data = new byte[union.DataLength];

                    //Переносим данные
                    int count = 0;
                    foreach (var id in ids)
                    {
                        for (int j = 0; j < entries[id].DataLength; ++j)
                            union.Data[j + count] = entries[id].Data[j];
                        count += entries[id].DataLength;
                    }

                    //Выводим результат
                    output(union);

                    // Назначить последний ID
                    i = ids.Last();
                }

                //Если обработан последний элемент - выходим
                if (i == idsToPrint.Last())
                    break;

                i = GetNextValueAfterOrReturnValue(idsToPrint, ids.Last(), idsToPrint.Last());
            }
        }

        private List<string> GetSources(List<LogEntry> logs)
        {
            List<string> ret = new List<string>();
            foreach (var Entry in logs)
                if (!ret.Contains(Entry.Serial))
                    ret.Add(Entry.Serial);
            return ret;
        }

        private List<int> GetEntriesWithSource(List<LogEntry> log, string source)
        {
            List<int> ret = new List<int>();
            for (int i = 0; i < log.Count; ++i)
                if (log[i].Serial == source)
                    ret.Add(i);
            return ret;
        }



        public List<LogEntry> ParseLog(in string rawLogs)
        {
            var result = new List<LogEntry>();

            string[] LogText = rawLogs.Split('\n');
            for (int i = 0; i < LogText.Length; ++i)
            {
                string[] temp = LogText[i].Split('\t');
                LogEntry entry = new LogEntry();

                for (int j = 0; j < temp.Length; ++j)
                {
                    string token = temp[j].Trim();

                    if (j == (int)LogField.Id)
                    {
                        if (int.TryParse(token, out int check))
                            entry.Id = check;
                        else
                            throw new InvalidDataException();
                    }
                    else if (j == (int)LogField.Time)
                    {
                        if (DateTime.TryParse(token, out DateTime t))
                            entry.Time = t;
                        else
                            throw new InvalidDataException();
                    }
                    else if (j == (int)LogField.AppName)
                    {
                        if (string.IsNullOrEmpty(token))
                            throw new InvalidDataException();
                        entry.App = token;
                    }
                    else if (j == (int)LogField.Command)
                    {
                        if (token == "IRP_MJ_WRITE")
                            entry.Direction = LogEntry.LogCommand.Request;
                        else if (token == "IRP_MJ_READ")
                            entry.Direction = LogEntry.LogCommand.Response;
                        else
                            throw new InvalidDataException();
                    }
                    else if (j == (int)LogField.Source)
                    {
                        if (string.IsNullOrEmpty(token))
                            throw new InvalidDataException();
                        else
                            entry.Serial = token;
                    }
                    else if (j == (int)LogField.Result)
                    {
                        if (token == "SUCCESS")
                            entry.Result = LogEntry.LogResult.Success;
                        else if (token == "TIMEOUT")
                            entry.Result = LogEntry.LogResult.Timeout;
                        else if (token == "FAIL")
                            entry.Result = LogEntry.LogResult.FAIL;
                        else
                            throw new InvalidDataException();
                    }
                    else if (j == (int)LogField.Data)
                    {
                        string[] data = token.Split(' ');
                        if ((data[0] != "Length") || (data[1].IndexOf(':') == -1))
                            throw new InvalidDataException();

                        if (int.TryParse(data[1].Substring(0, data[1].Length - 1), out int check))
                            entry.DataLength = check;
                        else
                            throw new InvalidDataException();

                        if ((data.Length - 2) != check)
                            throw new InvalidDataException();

                        entry.Data = new byte[check];
                        for (int k = 0; k < data.Length - 2; ++k)
                        {
                            bool isParced = byte.TryParse(data[k + 2], NumberStyles.HexNumber,
                                                        null, out byte byteValue);
                            if (isParced)
                                entry.Data[k] = byteValue;
                            else
                                throw new InvalidDataException();
                        }
                    }
                }
                result.Add(entry);
            }
            return result;
        }

        private List<InfoEntry> LoadInfoFile(in string path)
        {
            List<InfoEntry> ret = new List<InfoEntry>();
            string[] lines = File.ReadAllText(path).Split('\n');
            foreach (var line in lines)
            {
                string[] temp = line.Trim().Split(new string[] { " - " }, StringSplitOptions.None);
                if (temp.Length != 2)
                    continue;

                InfoEntry entry = new InfoEntry();
                for (int i = 0; i < temp.Length; ++i)
                {
                    string token = temp[i].Trim();
                    if (i == 0)
                    {
                        int check;
                        if (int.TryParse(token, out check))
                            entry.Id = check;
                        else
                            throw new InvalidDataException();
                    }
                    else
                        entry.Text = token;
                }
                ret.Add(entry);
            }
            return ret;
        }

        public bool LoadCommands(in string path)
        {
            m_commands = LoadInfoFile(path);
            return m_commands.Count > 0;
        }

        public bool LoadExceptions(in string path)
        {
            m_exceptions = LoadInfoFile(path);
            return m_exceptions.Count > 0;
        }

        private string GetCommandText(int command)
        {
            foreach (var i in m_commands)
                if (i.Id.ToString() == Convert.ToString(command, 16))
                    return i.Text;
            return "Unknown";
        }

        private string GetExceptionText(int command)
        {
            foreach (var i in m_exceptions)
                if (i.Id.ToString() == Convert.ToString(command, 16))
                    return i.Text;
            return "Unknown";
        }

        public string ConverToTXT(in List<LogEntry> log)
        {
            string result = "SourceType: Com";

            var sources = GetSources(log);

            Action<LogEntry> write = (LogEntry entry) => result += FormatForTXT(entry);

            foreach (var source in sources)
            {
                result += $"\n\tSource: {source} Speed = Unknown";
                List<int> idsToPrint = GetEntriesWithSource(log, source);
                Process(idsToPrint, log, write);
            }
            return result;
        }

        private string FormatForTXT(in LogEntry entry)
        {
            string result = $"\n\t\tLine: Direction={entry.Direction}";
            if ((entry.Data.Length > 0) && (Utility.CheckBit(entry.Data[1])))
            {
                result += $" Exception={GetExceptionText(entry.Data[1])}";
                return result;
            }
            if (entry.Result == LogEntry.LogResult.Timeout)
            {
                result += $" Error={entry.Result}";
                return result;
            }
            result += $" Address={entry.Data[0].ToString("X2").ToUpper()}" +
            $" Command='{entry.Data[1].ToString("X2")}: {GetCommandText(entry.Data[1])}'" +
            $" CRC= ";
            foreach (var a in Utility.CRC16(entry.Data))
                result += a.ToString("X2").ToUpper();
            if (entry.DataLength > 0)
            {
                result += $"\n\t\t\tRawFrame: ";
                for (int k = 0; k < entry.DataLength; ++k)
                    result += entry.Data[k].ToString("X2") + " ";
                result += $"\n\t\t\tRawData: ";
                for (int g = 2; g < entry.DataLength - 2; g++)
                    result += entry.Data[g].ToString("X2") + " ";
            }
            return result;
        }

        private List<InfoEntry> m_commands;
        private List<InfoEntry> m_exceptions;

        public LogConverter()
        {
            m_commands = new List<InfoEntry>();
            m_exceptions = new List<InfoEntry>();
        }
    }

}

