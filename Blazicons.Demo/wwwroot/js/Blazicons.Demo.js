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

    downloadSvgAsPng: function (svgContent, fileName, size, backgroundColor, cornerRadius, padding) {
        return new Promise(function (resolve, reject) {
            var canvas = document.createElement('canvas');
            canvas.width = size;
            canvas.height = size;
            var ctx = canvas.getContext('2d');
            var svgBlob = new Blob([svgContent], { type: 'image/svg+xml;charset=utf-8' });
            var url = URL.createObjectURL(svgBlob);
            var img = new Image();
            img.onload = function () {
                var pad = (padding && padding > 0) ? Math.min(padding, size / 2) : 0;
                var iconX = pad;
                var iconY = pad;
                var iconSize = size - pad * 2;
                var hasBackground = backgroundColor && backgroundColor.trim() !== '';
                var radius = (hasBackground && cornerRadius > 0) ? Math.min(cornerRadius, size / 2) : 0;
                if (radius > 0) {
                    ctx.beginPath();
                    ctx.moveTo(radius, 0);
                    ctx.lineTo(size - radius, 0);
                    ctx.arcTo(size, 0, size, radius, radius);
                    ctx.lineTo(size, size - radius);
                    ctx.arcTo(size, size, size - radius, size, radius);
                    ctx.lineTo(radius, size);
                    ctx.arcTo(0, size, 0, size - radius, radius);
                    ctx.lineTo(0, radius);
                    ctx.arcTo(0, 0, radius, 0, radius);
                    ctx.closePath();
                    ctx.clip();
                }
                if (hasBackground) {
                    ctx.fillStyle = backgroundColor;
                    ctx.fillRect(0, 0, size, size);
                }
                ctx.drawImage(img, iconX, iconY, iconSize, iconSize);
                URL.revokeObjectURL(url);
                canvas.toBlob(function (blob) {
                    var a = document.createElement('a');
                    var blobUrl = URL.createObjectURL(blob);
                    a.href = blobUrl;
                    a.download = fileName;
                    document.body.appendChild(a);
                    a.click();
                    document.body.removeChild(a);
                    URL.revokeObjectURL(blobUrl);
                    resolve();
                }, 'image/png');
            };
            img.onerror = reject;
            img.src = url;
        });
    },
}