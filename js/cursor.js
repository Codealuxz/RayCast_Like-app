document.addEventListener('DOMContentLoaded', () => {
    // Sélection de l'élément curseur
    const cursor = document.querySelector('.cursor');

    // Création des éléments du curseur personnalisé seulement si le curseur existe
    if (!cursor) return;

    const point = document.createElement('div');
    point.classList.add('cursor-point');

    const circle = document.createElement('div');
    circle.classList.add('cursor-circle');

    // Création de l'élément texte "scroll down" qui tourne
    const scrollText = document.createElement('div');
    scrollText.classList.add('cursor-scroll-text');

    // Créer le texte circulaire autour du cercle
    const text = "SCROLL DOWN - SCROLL DOWN - ";
    const textLength = text.length;
    const radius = 70; // Rayon du cercle de texte

    for (let i = 0; i < textLength; i++) {
        const angle = i * (360 / textLength);
        const charContainer = document.createElement('div');
        charContainer.className = 'char-container';
        charContainer.style.transform = `rotate(${angle}deg)`;

        const char = document.createElement('span');
        char.className = 'scroll-char';
        char.textContent = text[i];
        char.style.transform = `translateY(${radius}px) rotate(180deg)`;

        charContainer.appendChild(char);
        scrollText.appendChild(charContainer);
    }

    // Ajout des éléments au curseur
    cursor.appendChild(point);
    cursor.appendChild(circle);
    cursor.appendChild(scrollText);

    // Variables pour le suivi fluide
    let mouseX = 0;
    let mouseY = 0;
    let circleX = 0;
    let circleY = 0;
    let animationFrameId = null;
    let isVisible = false;
    let scrollAngle = 0;

    // Vérifier si l'appareil est tactile
    const isTouchDevice = 'ontouchstart' in window || navigator.maxTouchPoints > 0;

    // Si l'appareil est tactile, désactiver l'effet du curseur
    if (isTouchDevice) {
        document.querySelector('.cursor').style.display = 'none'; // Masquer le curseur
    } else {
        // Ne pas initialiser le curseur personnalisé sur les appareils tactiles
        if (isTouchDevice) {
            cursor.style.display = 'none';
            return;
        }

        // Fonction pour vérifier le défilement et masquer le texte
        function checkScrollPosition() {
            // Calculer 1% de la hauteur totale de la page
            const scrollThreshold = Math.max(
                document.body.scrollHeight,
                document.documentElement.scrollHeight
            ) * 0.01;

            // Si le défilement est supérieur à 1%, masquer le texte
            if (window.scrollY > scrollThreshold) {
                scrollText.style.opacity = '0';
                scrollText.style.fontSize = '49.3px';

            } else {
                scrollText.style.opacity = '1';
                scrollText.style.fontSize = '11.9px';
            }
        }

        // Appel initial pour vérifier le défilement
        checkScrollPosition();

        // Vérifier la position lors du défilement
        window.addEventListener('scroll', checkScrollPosition);

        // Masquer le curseur par défaut
        document.body.style.cursor = 'none';

        // Suivi de la position de la souris avec throttling
        let lastMoveTime = 0;

        // Fonction améliorée pour vérifier si un élément est cliquable
        function isClickable(element) {
            if (!element) return false;

            // Liste de tous les sélecteurs d'éléments cliquables
            const clickableSelectors = 'a, button, input, textarea, select, [role="button"], .logo, .project, .radio-container label, .radio-container input, [onclick], [data-clickable="true"]';

            // Vérifier l'élément lui-même
            if (element.matches(clickableSelectors)) return true;

            // Vérifier si l'élément a un pointeur spécifique en CSS
            const computedStyle = window.getComputedStyle(element);
            if (computedStyle.cursor === 'pointer') return true;

            // Vérifier tous les parents (pas seulement 3 niveaux)
            let parent = element.parentElement;
            while (parent) {
                if (parent.matches(clickableSelectors)) return true;

                const parentStyle = window.getComputedStyle(parent);
                if (parentStyle.cursor === 'pointer') return true;

                // Vérifier également les attributs onclick
                if (parent.hasAttribute('onclick')) return true;

                parent = parent.parentElement;
            }

            return false;
        }

        // Utiliser document.elementsFromPoint pour une meilleure détection
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

            // Vérifier tous les éléments à cette position (pas seulement e.target)
            const elementsAtPoint = document.elementsFromPoint(mouseX, mouseY);
            let isElementClickable = false;

            for (let i = 0; i < elementsAtPoint.length; i++) {
                if (isClickable(elementsAtPoint[i])) {
                    isElementClickable = true;
                    break;
                }
            }

            if (isElementClickable) {
                cursor.classList.add('cursor-hover');
                scrollText.style.opacity = '0';
                scrollText.style.fontSize = '49.3px';
            } else {
                cursor.classList.remove('cursor-hover');
                scrollText.style.fontSize = '11.9px';
                checkScrollPosition();
            }
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

            // Animation de rotation du texte autour du cercle
            scrollAngle += 0.5;
            scrollText.style.transform = `translate(${circleX}px, ${circleY}px) rotate(${scrollAngle}deg)`;

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
            point.style.transition = `width 0.2s ease, height 0.2s ease, top 0.1s ease, left 0.1s ease`;
            circle.classList.add('cursor-circle-clicked');
            circle.style.transition = `width 0.2s ease, height 0.2s ease, top 0.1s ease, left 0.1s ease`;
            scrollText.style.opacity = `0`;
        });

        document.addEventListener('mouseup', () => {
            point.classList.remove('cursor-point-clicked');
            circle.classList.remove('cursor-circle-clicked');
            if (scrollText.style.fontSize !== `49.3px`) {
                scrollText.style.opacity = `1`;
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
    }
});


function toggleMenu() {
    const nav = document.querySelector('.nav');
    nav.classList.toggle('active'); // Ajoute ou enlève la classe active
}


// Fonction à appeler pour naviguer avec la transition
function navigateWithTransition(url) {
    window.location.href = url;
}
