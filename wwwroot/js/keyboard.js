(function () {
    window._bb_keyboard = {
        attach: function (dotNetObj) {
            function onKey(e) {
                // space or Enter
                if (e.code === 'Space' || e.code === 'Enter') {
                    dotNetObj.invokeMethodAsync('OnSpacePressed').catch(function (err) { console.error(err); });
                }
            }
            window.addEventListener('keydown', onKey);
            return {
                dispose: function () { window.removeEventListener('keydown', onKey); }
            };
        }
    };
})();
