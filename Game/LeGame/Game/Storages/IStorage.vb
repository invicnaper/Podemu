Namespace Game.Interactives
    Public interface IStorage
        Sub BeginTrade(ByVal Client As GameClient)
        Sub EndTrade(ByVal Client As GameClient)
        Sub SetKamas(ByVal Client As GameClient, ByVal Kamas As Integer)
        Sub AddItem(ByVal Client As GameClient, ByVal ItemId As Integer, ByVal Quantity As Integer)
        Sub RemoveItem(ByVal Client As GameClient, ByVal ItemId As Integer, ByVal Quantity As Integer)
    end interface
End NameSpace