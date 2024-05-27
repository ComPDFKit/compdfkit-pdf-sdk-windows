using System;

namespace ComPDFKit.Tool.UndoManger
{
	public class ReferenceObject<T>
    {
        public class ReferenceChange<T>
        {
            public T oldValue { get; set; }
            public T newValue { get; set; }
        }

        private static event EventHandler<ReferenceChange<T>> Changed;
        
        public T Data { get;private set; }

        public ReferenceObject()
        {
            Changed += ReferenceObject_Changed;
        }

         ~ReferenceObject()
        {
            Changed -= ReferenceObject_Changed;
        }
        private void ReferenceObject_Changed(object sender, ReferenceChange<T> newData)
        {
            if(Data==null && newData.oldValue==null)
            {
                Data = newData.newValue;
                return;
            }
            if (Data.Equals(newData.oldValue))
            {
                Data = newData.newValue;
            }
        }

        public void Initialize(T newData)
        {
            Data = newData;
        }

        public void Update(T newData)
        {
            ReferenceChange<T> changeData = new ReferenceChange<T>()
            {
                oldValue = Data,
                newValue = newData
            };

            Changed?.Invoke(this, changeData);
        }
    }
}
