// Welcome Slider JavaScript
(function () {
    let currentSlide = 0;
    const slides = document.querySelectorAll('.slide');
    const indicators = document.querySelectorAll('.indicator');
    const btnNext = document.getElementById('btnNext');
    const btnSkip = document.getElementById('btnSkip');
    const btnStart = document.getElementById('btnStart');
    const totalSlides = slides.length;

    // Initialize
    function init() {
        showSlide(0);

        // Check for tokens in URL (from Login redirect)
        const params = new URLSearchParams(location.search);
        const accessToken = params.get('accessToken');
        const refreshToken = params.get('refreshToken');

        if (accessToken && refreshToken) {
            localStorage.setItem('accessToken', accessToken);
            localStorage.setItem('refreshToken', refreshToken);
            // Clean URL
            window.history.replaceState({}, document.title, window.location.pathname);
        }

        // Event listeners
        btnNext.addEventListener('click', nextSlide);
        btnSkip.addEventListener('click', skipToEnd);
        btnStart.addEventListener('click', goToNextStep);

        indicators.forEach((indicator, index) => {
            indicator.addEventListener('click', () => showSlide(index));
        });

        // Swipe support for mobile
        let touchStartX = 0;
        let touchEndX = 0;

        const slidesWrapper = document.getElementById('slidesWrapper');
        slidesWrapper.addEventListener('touchstart', e => {
            touchStartX = e.changedTouches[0].screenX;
        });

        slidesWrapper.addEventListener('touchend', e => {
            touchEndX = e.changedTouches[0].screenX;
            handleSwipe();
        });

        function handleSwipe() {
            if (touchEndX < touchStartX - 50) nextSlide();
            if (touchEndX > touchStartX + 50) prevSlide();
        }
    }

    function showSlide(index) {
        // Remove active class from all slides
        slides.forEach(slide => {
            slide.classList.remove('active', 'prev');
        });

        // Remove active class from all indicators
        indicators.forEach(indicator => {
            indicator.classList.remove('active');
        });

        // Add active class to current slide and indicator
        slides[index].classList.add('active');
        indicators[index].classList.add('active');

        currentSlide = index;

        // Show/hide buttons based on slide
        if (index === totalSlides - 1) {
            btnNext.classList.add('hidden');
            btnStart.classList.remove('hidden');
        } else {
            btnNext.classList.remove('hidden');
            btnStart.classList.add('hidden');
        }
    }

    function nextSlide() {
        if (currentSlide < totalSlides - 1) {
            slides[currentSlide].classList.add('prev');
            showSlide(currentSlide + 1);
        }
    }

    function prevSlide() {
        if (currentSlide > 0) {
            showSlide(currentSlide - 1);
        }
    }

    function skipToEnd() {
        goToNextStep();
    }

    async function goToNextStep() {
        // Update onboarding step
        try {
            await fetch('/api/onboarding/step', {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    step: 2 // BasicSettings
                })
            });
        } catch (error) {
            console.error('Error updating step:', error);
        }

        // Navigate to next page
        window.location.href = '/Onboarding/BasicSettings';
    }

    function getCookie(name) {
        const value = `; ${document.cookie}`;
        const parts = value.split(`; ${name}=`);
        if (parts.length === 2) return parts.pop().split(';').shift();
        return null;
    }

    // Initialize when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})();
