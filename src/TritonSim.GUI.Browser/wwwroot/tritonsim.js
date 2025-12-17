// tritonsim.js

export const canvasProviderModule = {
    createCanvas: (parentId, canvasId) => {
        const parent = document.getElementById(parentId);
        if (!parent) {
            console.error(`Parent element ${parentId} not found`);
            return null;
        }

        const canvas = document.createElement('canvas');
        canvas.id = canvasId;

        canvas.style.position = 'absolute';
        canvas.style.backgroundColor = 'transparent';
        canvas.style.border = '0px';
        canvas.style.pointerEvents = 'auto';
        canvas.style.zIndex = '1';

        // Disable right-click menu
        canvas.addEventListener('contextmenu', (e) => e.preventDefault());

        parent.appendChild(canvas);

        console.log(`Created render canvas: ${canvasId}`);
        return canvas;
    },

    updatePosition: (canvasId, x, y, width, height) => {
        const canvas = document.getElementById(canvasId);
        if (!canvas) {
            console.error(`Canvas ${canvasId} not found for position update`);
            return false;
        }

        // Update CSS layout
        canvas.style.left = `${x}px`;
        canvas.style.top = `${y}px`;
        canvas.style.width = `${width}px`;
        canvas.style.height = `${height}px`;

        console.log(`Updated canvas position: ${x}, ${y}, ${width}x${height}`);
        return true;
    },

    removeCanvas: (canvasId) => {
        const canvas = document.getElementById(canvasId);
        if (canvas) {
            canvas.remove();
            console.log(`Removed render canvas: ${canvasId}`);
            return true;
        }
        return false;
    }
};