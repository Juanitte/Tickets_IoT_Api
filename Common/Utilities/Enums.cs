using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Utilities
{
    /// <summary>
    ///     Tipos de prioridades de incidencia
    /// </summary>
    public enum Priorities
    {
        NOT_SURE,
        LOWEST,
        LOW,
        MEDIUM,
        HIGH,
        HIGHEST
    }

    /// <summary>
    ///     Tipos de estados de incidencia
    /// </summary>
    public enum States
    {
        PENDING,
        OPENED,
        PAUSED,
        FINISHED
    }

    public enum Language
    {
        English = 1,
        Spanish = 2
    }
}
