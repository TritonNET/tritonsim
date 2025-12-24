export const canvasProviderModule = {
    createCanvas(parentId, canvasId, width, height) {
        try {
            console.log(`Creating render canvas: ${canvasId} under parent: ${parentId} with width: ${width} and height: ${height}`);

            const parent = document.getElementById(parentId);
            if (!parent) {
                console.error(`Parent element ${parentId} not found`);
                return null;
            }

            const canvas = document.createElement('canvas');
            canvas.id = canvasId;

            // Disable right-click menu
            //canvas.addEventListener('contextmenu', (e) => e.preventDefault());

            parent.appendChild(canvas);

            console.log(`Created render canvas: ${canvasId}`);

            return canvas;
        }
        catch (error) {
            console.error(`Error creating canvas ${canvasId}:`, error);
            return null;
        }
    },

    updatePosition(canvasId, x, y, width, height) {
        console.log(`Updating canvas position: ${canvasId} to x: ${x}, y: ${y}, width: ${width}, height: ${height}`);

        const canvas = document.getElementById(canvasId);
        if (!canvas) {
            console.error(`Canvas ${canvasId} not found for position update`);
            return false;
        }

        const dpr = window.devicePixelRatio || 1;

        // Update buffer size
        const bufferWidth = Math.floor(width * dpr);
        const bufferHeight = Math.floor(height * dpr);

        if (canvas.width !== bufferWidth || canvas.height !== bufferHeight) {
            canvas.width = bufferWidth;
            canvas.height = bufferHeight;
            console.log(`Updated canvas position: ${x}, ${y}, ${width}x${height} (buffer: ${bufferWidth}x${bufferHeight})`);
        }

        return true;
    },

    removeCanvas(canvasId) {
        const canvas = document.getElementById(canvasId);
        if (canvas) {
            canvas.remove();
            console.log(`Removed canvas: ${canvasId}`);
            return true;
        }
        return false;
    }
};
