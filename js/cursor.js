document.addEventListener('DOMContentLoaded', () => {
    // Sélection de l'élément curseur
    const cursor = document.querySelector('.cursor');

    // Création des éléments du curseur personnalisé seulement si le curseur existe
    if (!cursor) return;

    const point = document.createElement('div');
    point.classList.add('cursor-point');

    const circle = document.createElement('div');
    circle.classList.add('cursor-circle');

    // Ajout des éléments au curseur
    cursor.appendChild(point);
    cursor.appendChild(circle);

    // Variables pour le suivi fluide
    let mouseX = 0;
    let mouseY = 0;
    let circleX = 0;
    let circleY = 0;
    let animationFrameId = null;
    let isVisible = false;

    // Vérifier si l'appareil est tactile
    const isTouchDevice = 'ontouchstart' in window || navigator.maxTouchPoints > 0;

    // Ne pas initialiser le curseur personnalisé sur les appareils tactiles
    if (isTouchDevice) {
        cursor.style.display = 'none';
        return;
    }

    // Masquer le curseur par défaut
    document.body.style.cursor = 'none';

    // Suivi de la position de la souris avec throttling
    let lastMoveTime = 0;
    document.addEventListener('mousemove', (e) => {
        // Limiter le taux de mise à jour à 60fps (environ 16ms)
        const now = performance.now();
        if (now - lastMoveTime < 16) return;
        lastMoveTime = now;

        if (!isVisible) {
            cursor.style.opacity = '1';
            isVisible = true;
        }

        mouseX = e.clientX;
        mouseY = e.clientY;

        // Le point suit directement la souris
        point.style.transform = `translate(${mouseX}px, ${mouseY}px)`;
    });

    // Animation optimisée pour le cercle qui suit plus lentement
    function animateCircle() {
        // Calcul de la position du cercle avec effet de retard
        const speed = 0.3;

        // Calcul des dimensions une seule fois en dehors de l'animation
        const circleWight = circle.clientWidth;
        const circleHeight = circle.clientHeight;
        const pointWight = point.clientWidth;
        const pointHeight = point.clientHeight;

        circleX += (mouseX - circleX - circleWight / 2 + pointWight / 2.5) * speed;
        circleY += (mouseY - circleY - circleHeight / 2 + pointHeight / 2.5) * speed;

        // Application de la transformation
        circle.style.transform = `translate(${circleX}px, ${circleY}px)`;

        animationFrameId = requestAnimationFrame(animateCircle);
    }

    // Démarrer l'animation
    animateCircle();

    // Optimisation: désactiver l'animation quand l'onglet n'est pas visible
    document.addEventListener('visibilitychange', () => {
        if (document.hidden) {
            cancelAnimationFrame(animationFrameId);
        } else {
            animationFrameId = requestAnimationFrame(animateCircle);
        }
    });

    // Animation au clic
    document.addEventListener('mousedown', () => {
        point.classList.add('cursor-point-clicked');
        circle.classList.add('cursor-circle-clicked');
    });

    document.addEventListener('mouseup', () => {
        point.classList.remove('cursor-point-clicked');
        circle.classList.remove('cursor-circle-clicked');
    });

    // Gestion des éléments cliquables avec délégation d'événement
    document.addEventListener('mouseover', (e) => {
        if (e.target.matches('a, button, input, textarea, select, [role="button"], .logo, .project, .radio-container label, .radio-container input')) {
            cursor.classList.add('cursor-hover');
        }
    });

    document.addEventListener('mouseout', (e) => {
        if (e.target.matches('a, button, input, textarea, select, [role="button"], .logo, .project, .radio-container label, .radio-container input')) {
            cursor.classList.remove('cursor-hover');
        }
    });

    // Cacher le curseur quand il quitte la fenêtre
    document.addEventListener('mouseleave', () => {
        cursor.style.opacity = '0';
        isVisible = false;
    });

    document.addEventListener('mouseenter', () => {
        cursor.style.opacity = '1';
        isVisible = true;
    });
});



