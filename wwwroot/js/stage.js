// Tiny helper: drives a requestAnimationFrame loop that calls into C# (Home.Tick)
// and reports window size changes. All physics lives in C#.
window.stage = (() => {
    let ref = null;
    let running = false;
    let wanted = false;
    let last = 0;
    let onResize = null;

    async function frame(t) {
        if (!ref) { running = false; return; }
        const dt = Math.min(Math.max((t - last) / 1000, 0), 1 / 30);
        last = t;
        wanted = false;

        let more = false;
        try { more = await ref.invokeMethodAsync('Tick', dt); }
        catch { more = false; }

        if ((more || wanted) && ref) requestAnimationFrame(frame);
        else running = false;
    }

    return {
        init(dotnetRef) {
            ref = dotnetRef;
            onResize = () => ref && ref.invokeMethodAsync('OnResize', innerWidth, innerHeight);
            addEventListener('resize', onResize);
            return [innerWidth, innerHeight];
        },
        start() {
            wanted = true;
            if (running || !ref) return;
            running = true;
            last = performance.now();
            requestAnimationFrame(frame);
        },
        dispose() {
            if (onResize) removeEventListener('resize', onResize);
            ref = null;
        }
    };
})();
