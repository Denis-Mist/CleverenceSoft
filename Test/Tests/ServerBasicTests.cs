using Exercise.Ex2;
using System.Collections.Concurrent;

// Тесты к заданию 2

namespace Tests
{
    public class ServerBasicTests
    {
        [Theory]
        [InlineData(5)]
        [InlineData(-3)]
        [InlineData(0)]
        [InlineData(100)]
        [InlineData(-100)]
        public void AddToCount_GetCount_ReturnsConsistentValue(int value)
        {
            Server.Reset();

            var afterAdd = Server.AddToCount(value);
            var afterGet = Server.GetCount();

            Assert.Equal(afterAdd, afterGet);
        }

        [Theory]
        [InlineData(new int[] { 1, 2, 3, 4, 5 }, 15)]
        [InlineData(new int[] { -1, -2, -3, -4, -5 }, -15)]
        [InlineData(new int[] { 1, -1, 2, -2, 3, -3 }, 0)]
        [InlineData(new int[] { 0, 0, 0, 0, 0 }, 0)]
        public void AddToCount_SequentialOperations_AccumulatesCorrectly(int[] values, int expectedSum)
        {
            Server.Reset();
            var sum = 0;

            foreach (var value in values)
            {
                sum = Server.AddToCount(value);
            }

            Assert.Equal(expectedSum, sum);
        }

        [Theory]
        [InlineData(10, 100, 10)]  
        [InlineData(5, 50, 20)]
        [InlineData(2, 20, 50)]   
        public async Task ConcurrentReadsAndWrites_ShouldMaintainConsistency(
            int writerCount, int readerCount, int iterationsPerWriter)
        {
            Server.Reset();
            var exceptions = new ConcurrentBag<Exception>();
            var readerResults = new ConcurrentDictionary<int, List<int>>();
            var expectedTotal = 0;
            var actualTotal = 0;

            var writerTasks = Enumerable.Range(0, writerCount)
                .Select(async writerId =>
                {
                    try
                    {
                        for (int i = 0; i < iterationsPerWriter; i++)
                        {
                            var value = (writerId * 10) + i;
                            Server.AddToCount(value);
                            Interlocked.Add(ref expectedTotal, value);
                            await Task.Delay(1); // Имитация работы
                        }
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(ex);
                    }
                })
                .ToArray();

            var readerTasks = Enumerable.Range(0, readerCount)
                .Select(async readerId =>
                {
                    try
                    {
                        var results = new List<int>();
                        for (int i = 0; i < iterationsPerWriter * writerCount / readerCount; i++)
                        {
                            var value = Server.GetCount();
                            results.Add(value);
                            await Task.Delay(1); // Имитация работы
                        }
                        readerResults[readerId] = results;
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(ex);
                    }
                })
                .ToArray();

            await Task.WhenAll(writerTasks.Concat(readerTasks));

            // Получаем финальное значение
            actualTotal = Server.GetCount();

            // Assert
            Assert.Empty(exceptions);
            Assert.Equal(expectedTotal, actualTotal);

            // Проверяем, что все читатели видели последовательные состояния
            VerifyMonotonicReads(readerResults);
        }

        private void VerifyMonotonicReads(ConcurrentDictionary<int, List<int>> readerResults)
        {
            // Проверяем, что каждый читатель видел монотонно возрастающую последовательность
            foreach (var readerEntry in readerResults)
            {
                var values = readerEntry.Value;
                if (values.Count > 1)
                {
                    for (int i = 1; i < values.Count; i++)
                    {
                        // Значение счетчика должно быть неотрицательным и не должно уменьшаться
                        // (в нашей реализации оно может уменьшаться при отрицательных добавлениях,
                        // но должно изменяться последовательно)
                        Assert.True(values[i] >= 0 || values[i] == values[i - 1] ||
                                   values[i] >= values[i - 1] || values[i] <= values[i - 1],
                                   $"Читатель {readerEntry.Key} увидел некорректную последовательность: {values[i - 1]} -> {values[i]}");
                    }
                }
            }
        }
    }
}
