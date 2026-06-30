window.fastRestaurantClock = {
    nowIso: () => new Date().toISOString(),
    timezoneOffsetMinutes: () => new Date().getTimezoneOffset()
};

window.fastRestaurantSound = {
    src: '/sounds/new-dish.mp3',
    _unlocked: false,

    unlock: function () {
        if (window.fastRestaurantSound._unlocked) return;
        window.fastRestaurantSound._unlocked = true;

        try {
            const audio = new Audio(window.fastRestaurantSound.src);
            audio.volume = 0;
            audio.play().then(function () { audio.pause(); }).catch(function () {});
        } catch (e) {}
    },

    playNewDish: function () {
        try {
            const audio = new Audio(window.fastRestaurantSound.src);
            audio.play().catch(function () {});
        } catch (e) {}
    }
};

document.addEventListener('pointerdown', function () { window.fastRestaurantSound.unlock(); }, { once: true });
document.addEventListener('keydown', function () { window.fastRestaurantSound.unlock(); }, { once: true });
