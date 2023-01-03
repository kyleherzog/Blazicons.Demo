window.blaziconsDemo = {
    addBodyClass: async function (className) {
        return new Promise(function (resolve) {
            document.body.classList.add(className);
            resolve();
        })
    },

    removeBodyClass: async function (className) {
        return new Promise(function (resolve) {
            document.body.classList.remove(className);
            resolve();
        })
    },
}