using System;

namespace ExRam.Gremlinq.Samples.Shared {
    public class IdGenerator {
        private static readonly object _syncLock = new object();


        private static string id;


        
        public static string GetId() {
            lock (_syncLock) {

                var guid = Guid.NewGuid();
                Console.WriteLine($"guid: {guid}");
                return guid.ToString();
                
            }
        }


        public static string GetRandomAllocatedId(int max = 0) {
            
            lock (_syncLock) {
                return new Random().Next(0, max).ToString();
            }
            
            
        }
    }
}
