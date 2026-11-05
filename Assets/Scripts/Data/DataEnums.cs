
public enum IntelThread { Convoy, Granary, Vaule, Route, Traitor, Report, Village }

public enum ClueKind { Who, What, Where, When, State, Evidence, Detail }

public enum Handler { None, Rook, Shrike }

public enum ShowCondition
{
    Always,
    DeliveredLastNight,
    NotDeliveredLastNight,
    RouteVisitedLastNight,
    HasFragment,
    DeliveredBy,
    SuspicionLow,
    SuspicionMid,
    SuspicionHigh,
    CorrectLastNight,
    WrongLastNight,
    CaseCorrect,
    CaseNeverDelivered,
    SaidThisVisit,
}

public enum RouteRisk { Safe, Low, Medium, High }

public enum SuspicionChannel { Emel, Perrin, Sable, Patrol }

public enum EndingId { Burned, Evicted, Defection, Extraction, LeftInCold }
