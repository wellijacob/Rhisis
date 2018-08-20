using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Rhisis.Core.Resources.Dyo
{
    public sealed class DyoFile : FileStream, IDisposable
    {
        private readonly ICollection<DyoElement> _elements;

        /// <summary>
        /// Gets the Dyo elements collection.
        /// </summary>
        public IReadOnlyCollection<DyoElement> Elements => this._elements as IReadOnlyCollection<DyoElement>;

        /// <summary>
        /// Creates a new <see cref="DyoFile"/> instance.
        /// </summary>
        /// <param name="path">Dyo file path</param>
        public DyoFile(string path)
            : base(path, FileMode.Open, FileAccess.Read)
        {
            this._elements = new List<DyoElement>();
            this.Read();
        }

        /// <summary>
        /// Reads the <see cref="DyoFile"/> contents.
        /// </summary>
        private void Read()
        {
            var reader = new BinaryReader(this);

            while (true)
            {
                DyoElement rgnElement = null;
                int type = reader.ReadInt32();

                if (type == -1)
                    break;
                if (type == 5)
                {
                    rgnElement = new NpcDyoElement();
                    rgnElement.Read(reader);
                }

                if (rgnElement != null)
                    this._elements.Add(rgnElement);
            }
        }

        /// <summary>
        /// Gets the specific elements.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IEnumerable<T> GetElements<T>() where T : DyoElement => this._elements.Where(x => x is T).Select(x => x as T);

        /// <summary>
        /// Dispose the <see cref="DyoFile"/> resources.
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                this._elements.Clear();

            base.Dispose(disposing);
        }
    }
}
