window.tableLayout = (() => {
    const storagePrefix = "restaurant-table-layout:v3:";

    function storageKey(location, tableId) {
        return `${storagePrefix}${location}:${tableId}`;
    }

    function gridPosition(index, count, columns, minX = 0, maxX = 100) {
        const rows = Math.ceil(count / columns);
        const column = index % columns;
        const row = Math.floor(index / columns);

        return {
            x: minX + ((column + 0.5) / columns) * (maxX - minX),
            y: ((row + 0.5) / rows) * 100
        };
    }

    function gardenPosition(index, count) {
        if (count <= 4) {
            const fourTableLayout = [
                { x: 18, y: 27 },
                { x: 82, y: 27 },
                { x: 33, y: 75 },
                { x: 70, y: 75 }
            ];
            return fourTableLayout[index];
        }

        const firstRingCount = Math.min(count, 8);
        const ring = index < 8 ? 0 : 1 + Math.floor((index - 8) / 12);
        const itemsInRing = ring === 0 ? firstRingCount : Math.min(12, count - 8 - ((ring - 1) * 12));
        const indexInRing = ring === 0 ? index : (index - 8) % 12;
        const angle = (-Math.PI / 2) + ((Math.PI * 2 * indexInRing) / Math.max(1, itemsInRing));
        const radiusX = Math.min(43, 33 + (ring * 9));
        const radiusY = Math.min(43, 34 + (ring * 8));

        return {
            x: 50 + Math.cos(angle) * radiusX,
            y: 50 + Math.sin(angle) * radiusY
        };
    }

    function defaultPosition(index, count, columns, location) {
        const normalizedLocation = location.toLocaleLowerCase("es");
        if (normalizedLocation === "patio")
            return gardenPosition(index, count);
        if (normalizedLocation === "terraza")
            return gridPosition(index, count, columns, 47, 84);
        if (normalizedLocation === "barra")
            return gridPosition(index, count, columns, 0, 84);
        return gridPosition(index, count, columns);
    }

    function readPosition(location, tableId) {
        try {
            const value = localStorage.getItem(storageKey(location, tableId));
            return value ? JSON.parse(value) : null;
        } catch {
            return null;
        }
    }

    function savePosition(location, tableId, position) {
        localStorage.setItem(storageKey(location, tableId), JSON.stringify(position));
    }

    function place(element, position) {
        element.style.left = `${position.x}%`;
        element.style.top = `${position.y}%`;
    }

    function horizontalBounds(location) {
        const normalizedLocation = location.toLocaleLowerCase("es");
        if (normalizedLocation === "terraza")
            return { min: 47, max: 84 };
        if (normalizedLocation === "barra")
            return { min: 0, max: 84 };
        return { min: 0, max: 100 };
    }

    function constrainPosition(element, container, position, location) {
        const halfWidthPercent = (element.offsetWidth / 2 / container.clientWidth) * 100;
        const halfHeightPercent = (element.offsetHeight / 2 / container.clientHeight) * 100;
        const bounds = horizontalBounds(location);
        return {
            x: Math.max(bounds.min + halfWidthPercent, Math.min(bounds.max - halfWidthPercent, position.x)),
            y: Math.max(halfHeightPercent, Math.min(100 - halfHeightPercent, position.y))
        };
    }

    function attachDrag(element, container, location) {
        if (element.dataset.dragReady === "true")
            return;

        element.dataset.dragReady = "true";
        let activePointer = null;

        element.addEventListener("pointerdown", event => {
            if (element.dataset.editable !== "true")
                return;

            activePointer = event.pointerId;
            element.setPointerCapture(activePointer);
            element.classList.add("dragging");
            event.preventDefault();
        });

        element.addEventListener("pointermove", event => {
            if (activePointer !== event.pointerId)
                return;

            const bounds = container.getBoundingClientRect();
            const halfWidth = element.offsetWidth / 2;
            const halfHeight = element.offsetHeight / 2;
            const allowed = horizontalBounds(location);
            const minXPixels = (allowed.min / 100) * bounds.width + halfWidth;
            const maxXPixels = (allowed.max / 100) * bounds.width - halfWidth;
            const xPixels = Math.max(minXPixels, Math.min(maxXPixels, event.clientX - bounds.left));
            const yPixels = Math.max(halfHeight, Math.min(bounds.height - halfHeight, event.clientY - bounds.top));
            const position = {
                x: (xPixels / bounds.width) * 100,
                y: (yPixels / bounds.height) * 100
            };

            place(element, position);
        });

        const finish = event => {
            if (activePointer !== event.pointerId)
                return;

            activePointer = null;
            element.classList.remove("dragging");
            const position = {
                x: parseFloat(element.style.left),
                y: parseFloat(element.style.top)
            };
            savePosition(location, element.dataset.tableId, position);
        };

        element.addEventListener("pointerup", finish);
        element.addEventListener("pointercancel", finish);
    }

    function initialize() {
        document.querySelectorAll(".tables-layout").forEach(container => {
            const location = container.dataset.location || "Sin sector";
            const tables = Array.from(container.querySelectorAll(".table-place"));
            const widestTable = Math.max(206, ...tables.map(table => table.offsetWidth));
            const tallestTable = Math.max(124, ...tables.map(table => table.offsetHeight));
            const usableWidth = location.toLocaleLowerCase("es") === "terraza"
                ? container.clientWidth * .53
                : container.clientWidth;
            const columns = Math.max(1, Math.min(4, Math.floor(usableWidth / (widestTable + 34))));
            const rows = Math.max(2, Math.ceil(tables.length / columns));
            const normalizedLocation = location.toLocaleLowerCase("es");
            const minimumHeight = normalizedLocation === "patio" && tables.length <= 4 ? 480 : 420;
            const contentHeight = normalizedLocation === "patio" && tables.length <= 4
                ? 480
                : rows * (tallestTable + 42);
            container.style.height = `${Math.max(minimumHeight, contentHeight)}px`;

            tables.forEach((element, index) => {
                const saved = readPosition(location, element.dataset.tableId);
                const position = saved || defaultPosition(index, tables.length, columns, location);
                place(element, constrainPosition(element, container, position, location));
                attachDrag(element, container, location);
            });
        });
    }

    function reset(location) {
        const prefix = `${storagePrefix}${location}:`;
        for (let index = localStorage.length - 1; index >= 0; index--) {
            const key = localStorage.key(index);
            if (key && key.startsWith(prefix))
                localStorage.removeItem(key);
        }
    }

    if (!window.__tableLayoutResizeReady) {
        window.__tableLayoutResizeReady = true;
        let resizeTimer;
        window.addEventListener("resize", () => {
            clearTimeout(resizeTimer);
            resizeTimer = setTimeout(initialize, 120);
        });
    }

    return { initialize, reset };
})();
