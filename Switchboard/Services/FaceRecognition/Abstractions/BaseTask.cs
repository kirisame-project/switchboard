using System;
using System.Text.Json.Serialization;

namespace Switchboard.Services.FaceRecognition.Abstractions
{
    internal class BaseTask
    {
        private BaseTaskState _state = BaseTaskState.Pending;

        [JsonPropertyName("state")]
        public BaseTaskState State
        {
            get => _state;
            set
            {
                _state = value;

                switch (value)
                {
                    case BaseTaskState.Running:
                        StartTime = DateTime.Now;
                        break;
                    case BaseTaskState.Succeeded:
                    case BaseTaskState.Failed:
                        Time = (int) (DateTime.Now - StartTime).TotalMilliseconds;
                        break;
                }

                OnStateChanged?.Invoke(this, value);
            }
        }

        [JsonIgnore] public bool IsCompleted => State == BaseTaskState.Succeeded || State == BaseTaskState.Failed;

        [JsonPropertyName("start")] public DateTime StartTime { get; private set; } = DateTime.UnixEpoch;

        [JsonPropertyName("time")] public int Time { get; set; } = int.MaxValue;

        public event EventHandler<BaseTaskState> OnStateChanged;
    }
}