using Rhisis.Core.Structures;

namespace Rhisis.Core.Resources
{
    /// <summary>
    /// "respawn7" region structure.
    /// </summary>
    /// <remarks>
    /// This region structure represente a monster spawner.
    /// </remarks>
    public sealed class RgnRespawn7 : RgnElement
    {
        public int Model { get; private set; }

        public int Count { get; private set; }

        public int Time { get; private set; }

        public int AgroNumber { get; private set; }

        public RgnRespawn7(string[] data)
        {
            this.Type = int.Parse(data[1]);
            this.Model = int.Parse(data[2]);
            this.Position = new Vector3(data[3], data[4], data[5]);
            this.Count = int.Parse(data[6]);
            this.Time = int.Parse(data[7]);
            this.AgroNumber = int.Parse(data[8]);
            this.Left = int.Parse(data[9]);
            this.Top = int.Parse(data[10]);
            this.Right = int.Parse(data[11]);
            this.Bottom = int.Parse(data[12]);
        }
    }
}
