// tritonsim.js

export const canvasProviderModule = {
    createCanvas: (parentId, canvasId) => {
        const parent = document.getElementById(parentId);
        if (!parent) {
            console.error(`Parent element ${parentId} not found`);
            return false;
        }

        // CHANGE: Create a 'canvas' element, not a 'div'
        const canvas = document.createElement('canvas');
        canvas.id = canvasId;

        // Style setup
        canvas.style.position = 'absolute';
        canvas.style.backgroundColor = '#1a1a1a';
        canvas.style.border = '3px solid lime'; // Visual indicator
        canvas.style.pointerEvents = 'auto';
        canvas.style.zIndex = '1';

        // OPTIONAL: Disable right-click menu (helpful for games/sims)
        canvas.addEventListener('contextmenu', (e) => e.preventDefault());

        parent.appendChild(canvas);

        console.log(`Created render canvas: ${canvasId}`);
        return true;
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

        // NOTE: If your simulation looks blurry, you may also need to set 
        // the internal resolution attributes to match the CSS size:
        // canvas.width = width;
        // canvas.height = height;

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