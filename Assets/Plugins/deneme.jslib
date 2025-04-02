mergeInto(LibraryManager.library, {
    SendData: function (json) {
        const data = JSON.parse(UTF8ToString(json));
        
        const nameField = document.getElementById("name");
        const idField = document.getElementById("ID");
        const pnrField = document.getElementById("PNr");
        const colorField = document.getElementById("color");
        
        nameField.innerText = data.Name;
        idField.innerText = data.Id;
        pnrField.innerText = data.Pnr;
        colorField.innerText = data.Color;
    },
});