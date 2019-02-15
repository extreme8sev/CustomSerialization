#region Usings

using System;
using System.IO;
using System.Text;

#endregion

namespace CustomSerialization
{
    internal class Program

    {
        #region  Public Methods

        public class ListNode
        {
            #region Public Members

            //Данные узла
            public string Data;

            //Ссылка на следующий узел
            public ListNode Next;

            //Ссылка на предыдующиий узел
            public ListNode Previous;

            //Ссылка на произвольный узел списка
            public ListNode Random;

            #endregion
        }

        /// <summary>
        /// Структура данных для хранения, сериализации и десериализации данных
        /// </summary>
        public class CustomTwoLinkedListWithRandomNode
        {
            #region Public Members
            /// <summary>
            /// Счётчик количества элементов
            /// </summary>
            public int Count;

            /// <summary>
            /// "Голова" списка
            /// </summary>
            public ListNode Head;

            /// <summary>
            /// "Хвост" списка
            /// </summary>
            public ListNode Tail;

            #endregion

            #region  Public Methods

            /// <summary>
            /// Метод десериализации данных в списке
            /// </summary>
            /// <param name="s"></param>
            public static CustomTwoLinkedListWithRandomNode Deserialize(FileStream s)
            {
                using (var binaryReader = new BinaryReader(s))
                {
                    //Чтение 32 бит для получения количества хранимых элементов
                    var storedElementsCount = binaryReader.ReadInt32();
                    //Инициализация одномерного массива заданной длинны для храниния считанных элеметнов
                    var deserializationArray = new ListNode[storedElementsCount];
                    //Инициализация одномерного массива заданной длинны для храниния индексов произвольных элементов
                    var randArray = new int[storedElementsCount];
                    //Инициализация текущего индекса
                    int i = 0;
                    //Указатель на текущий элемент
                    ListNode node = null;


                    //Последовательное считывание бинарных данных из массива с помощью цикла с постусловием
                    do
                    {
                        //Чтение 32 бит для получения длинны хранимого сообщения
                        var storedDataLength = binaryReader.ReadInt32();

                        //Чтение массива символов заданной длинны
                        var data = new string(binaryReader.ReadChars(storedDataLength));
                        //Чтение индекса произвольного элемента
                        randArray[i] = binaryReader.ReadInt32();

                        //Создание связей
                        //Первый элемент
                        if (node == null)
                        {
                            node = new ListNode { Data = data };
                        }
                        //Любой последующий
                        else
                        {
                            node = node.Next = new ListNode { Data = data };
                        }

                        //Запись полученного элемента в массив для хранения считанных элементов
                        deserializationArray[i] = node;
                        //Инкрементация индекса
                        i++;
                    } while (i < storedElementsCount);

                    //Дополнительный цикл для восстановления ссылок на произвольные элементы
                    for (i = 0; i < deserializationArray.Length; i++)
                    {
                        deserializationArray[i].Random = deserializationArray[randArray[i]];
                    }

                    return new CustomTwoLinkedListWithRandomNode
                    {
                        Head = deserializationArray[0],
                        Tail = deserializationArray[storedElementsCount-1],
                        Count = storedElementsCount
                    };
                }
            }

            /// <summary>
            /// Функция для вывода данных
            /// </summary>
            public void Print()
            {
                ListNode node = Head;
                do
                {
                    Console.WriteLine($"Данные узла: {node.Data}, данные узла по произвольной ссылке: {node.Random.Data}");
                }
                while ((node = node.Next) != null);
            }

            /// <summary>
            /// Инициализация экземпляра с тестовым набором данных
            /// </summary>
            /// <returns></returns>
            public static CustomTwoLinkedListWithRandomNode GetTestInstance()
            {
                var list = new CustomTwoLinkedListWithRandomNode();

                //Создание узлов
                var node1 = new ListNode { Previous = null, Data = "node_0" };
                var node2 = new ListNode { Previous = node1, Data = "node_1" };
                var node3 = new ListNode { Previous = node2, Data = "node_3", Next = null };

                //Создание связей между узлами
                node1.Next = node2;
                node2.Next = node3;
                node1.Random = node3;
                node2.Random = node1;
                node3.Random = node2;

                //Указание дополнительных данных для класса-обработчика
                list.Head = node1;
                list.Tail = node3;

                //Указание размерности
                list.Count = 3;

                return list;
            }

            /// <summary>
            /// Метод сериализации данных в списке
            /// </summary>
            /// <param name="s"></param>
            public void Serialize(FileStream s)
            {
                using (var binaryWriter = new BinaryWriter(s))
                {
                    //Перевод списка в одномерный массив заданной длины
                    var nodeArray = new ListNode[Count];
                    var i = 0;
                    var node = Head;
                    do
                    {
                        nodeArray[i] = node;
                        i++;
                    } while ((node = node.Next) != null);

                    //Запись 32 бит для обозначения количества элементов
                    binaryWriter.Write(Count);
                    //Запись данных
                    foreach (var arrNode in nodeArray)
                    {
                        //Запись количества символов
                        binaryWriter.Write(arrNode.Data.Length);
                        //Запись всех символов
                        binaryWriter.Write(arrNode.Data.ToCharArray());
                        //Запись индекса произвольного элемента
                        binaryWriter.Write(Array.IndexOf(nodeArray,
                                                         arrNode.Random));
                    }
                }
            }

            #endregion
        }

        #endregion

        #region  Private Methods

        private static void Main()

        {
            //Установка кодировки для отображения кириллицы
            Console.OutputEncoding = Encoding.UTF8;

            //Объявление путей
            var taskPath = "C:/SaberInteractive/TestTask";
            var fileName = "data.bin";

            //Создание директории
            if (!Directory.Exists(taskPath))
            {
                Directory.CreateDirectory(taskPath);
            }

            //Инициализация экземпляра с тестовыми данными
            var customTwoLinkedListWithRandomNode = CustomTwoLinkedListWithRandomNode.GetTestInstance();

            Console.WriteLine("Инициализированный список:");
            customTwoLinkedListWithRandomNode.Print();

            //Сериализация в массив байт
            customTwoLinkedListWithRandomNode.Serialize(File.Open(Path.Combine(taskPath, fileName),
                                         FileMode.OpenOrCreate,
                                         FileAccess.ReadWrite));
            Console.WriteLine("Восстановленный список:");
            //Десериализация из массива байт
            var deserializedList = CustomTwoLinkedListWithRandomNode.Deserialize(File.Open(Path.Combine(taskPath, fileName),
                                           FileMode.OpenOrCreate,
                                           FileAccess.Read));
            deserializedList.Print();

            Console.WriteLine("Нажмите любую кнопку для завершения программы.");
            //Ожидание действия пользователя
            Console.Read();
        }

        #endregion
    }
}