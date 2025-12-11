document.addEventListener('DOMContentLoaded', function () {
    // 1. Initialize Snowfall
    const snowContainer = document.getElementById('snow-container');
    if (snowContainer) {
        initSnowfall(snowContainer);
    }
});

function initSnowfall(container) {
    const snowflakeCount = 100; // Increased count for particle effect

    for (let i = 0; i < snowflakeCount; i++) {
        createSnowflake(container);
    }
}

function createSnowflake(container) {
    const snowflake = document.createElement('div');
    snowflake.classList.add('snowflake');

    // Random Properties for natural look
    // Size: small dots, between 2px and 6px
    const sizeVal = Math.random() * 4 + 2;
    const size = sizeVal + 'px';

    // Position: anywhere across the screen width
    const posX = Math.random() * 100 + 'vw';

    // Delay: random start times to prevent "clumping" at start
    const delay = Math.random() * 15 + 's';

    // Duration: 10s to 25s for varied fall speeds
    const duration = Math.random() * 15 + 10 + 's';

    // Opacity: slight variance
    const opacity = Math.random() * 0.5 + 0.5;

    snowflake.style.width = size;
    snowflake.style.height = size;
    snowflake.style.left = posX;
    snowflake.style.animationDelay = delay;
    snowflake.style.animationDuration = duration;
    snowflake.style.opacity = opacity;

    container.appendChild(snowflake);
}
