export const canvasProviderModule = {
    createCanvas: (parentId, canvasId) => {
        const parent = document.getElementById(parentId);
        if (!parent) {
            console.error(`Parent element ${parentId} not found`);
            return null;
        }

        const canvas = document.createElement('canvas');
        canvas.id = canvasId;

        // Set canvas attributes for proper WebGL context
        canvas.setAttribute('data-engine', 'bgfx');

        canvas.style.position = 'absolute';
        canvas.style.backgroundColor = 'transparent';
        canvas.style.border = '0px';
        canvas.style.pointerEvents = 'auto';
        canvas.style.zIndex = '1';

        // Disable right-click menu
        canvas.addEventListener('contextmenu', (e) => e.preventDefault());

        parent.appendChild(canvas);

        console.log(`Created render canvas: ${canvasId}`);

        // IMPORTANT: Register canvas with Emscripten
        if (typeof Module !== 'undefined' && Module.canvas) {
            // Emscripten will use this canvas
            Module.canvas = canvas;
        } else {
            // Create global reference for Emscripten
            window.tritonsimCanvas = canvas;
        }

        const glAttribs = {
            alpha: false,
            depth: true,
            stencil: true,
            antialias: false,
            preserveDrawingBuffer: false,
            powerPreference: 'high-performance',
            failIfMajorPerformanceCaveat: true
        };

        let gl = null;
        try {
            gl = canvas.getContext('webgl2', glAttribs);
            if (gl) {
                console.log('WebGL2 context created successfully');
                console.log('WebGL Vendor:', gl.getParameter(gl.VENDOR));
                console.log('WebGL Version:', gl.getParameter(gl.VERSION));
            } else {
                console.warn('WebGL2 not available, trying WebGL1...');
                gl = canvas.getContext('webgl', glAttribs);
                if (gl) {
                    console.log('WebGL1 context created successfully');
                }
            }
        } catch (e) {
            console.error('Error creating WebGL context:', e);
        }

        if (!gl) {
            console.error('FAILED: Could not create any WebGL context');
            canvas.remove();
            return null;
        }

        // Store context for debugging
        window.tritonsimGL = gl;

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

        // Update canvas drawing buffer size
        const dpr = window.devicePixelRatio || 1;
        canvas.width = Math.floor(width * dpr);
        canvas.height = Math.floor(height * dpr);

        console.log(`Updated canvas position: ${x}, ${y}, ${width}x${height} (buffer: ${canvas.width}x${canvas.height})`);
        return true;
    },

    removeCanvas: (canvasId) => {
        const canvas = document.getElementById(canvasId);
        if (canvas) {
            // Clean up Emscripten reference
            if (typeof Module !== 'undefined' && Module.canvas === canvas) {
                Module.canvas = null;
            }
            if (window.tritonsimCanvas === canvas) {
                window.tritonsimCanvas = null;
            }

            canvas.remove();
            console.log(`Removed render canvas: ${canvasId}`);
            return true;
        }
        return false;
    },

    // Helper function to get the canvas for Emscripten
    getCanvas: () => {
        const canvas = document.getElementById('tbgfxcs');
        if (canvas) return canvas;

        // Fallback to global reference
        return window.tritonsimCanvas || null;
    }
};