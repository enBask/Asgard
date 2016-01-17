var asgard = require('asgard');
var entityMgr = require('entityManager');

var i = setInterval(createTestObject, 1000);

function createTestObject() {

    var e = asgard.CreateTestObject();
    
    setTimeout(function () {
        asgard.DestoryTestObject(e);
    }, 10000);
    
    

    //clearTimeout(i);
}

function onAsgardTick(delta) {

    /*
    var sys = asgard.getSystem('Asgard.Core.Physics.Midgard');
    console.log(sys.BodyList.length);
    var aspect = entityMgr.AspectAll("Shared.RenderData");
    var ents = entityMgr.GetEntities(aspect);

    var compType = entityMgr.getComponentType("Shared.RenderData");

    for (var i = 0; i < ents.length; ++i)
    {
        var e = ents[i];
        var comp = e.GetComponent(compType);
        var pos = comp.GetPosition();
        console.log(pos.ToString());
    }
    */
}
asgard.OnTick = onAsgardTick;
