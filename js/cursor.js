document.addEventListener('DOMContentLoaded', () => {
    // Sélection de l'élément curseur
    const cursor = document.querySelector('.cursor');

    // Création des éléments du curseur personnalisé
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

    // Masquer le curseur par défaut
    document.body.style.cursor = 'none';

    // Suivi de la position de la souris
    document.addEventListener('mousemove', (e) => {
        mouseX = e.clientX;
        mouseY = e.clientY;

        // Le point suit directement la souris
        point.style.transform = `translate(${mouseX}px, ${mouseY}px)`;
    });

    // Animation pour le cercle qui suit plus lentement
    function animateCircle() {
        // Calcul de la position du cercle avec effet de retard
        const speed = 0.3;
        circleWight = circle.clientWidth;
        circleHeight = circle.clientHeight;
        pointWight = point.clientWidth;
        pointHeight = point.clientHeight;

        circleX += (mouseX - circleX - circleWight / 2 + pointWight / 2.5) * speed;
        circleY += (mouseY - circleY - circleHeight / 2 + pointHeight / 2.5) * speed;

        // Application de la transformation
        circle.style.transform = `translate(${circleX}px, ${circleY}px)`;

        requestAnimationFrame(animateCircle);
    }

    // Démarrer l'animation
    animateCircle();

    // Animation au clic
    document.addEventListener('mousedown', () => {
        point.classList.add('cursor-point-clicked');
        circle.classList.add('cursor-circle-clicked');
    });

    document.addEventListener('mouseup', () => {
        point.classList.remove('cursor-point-clicked');
        circle.classList.remove('cursor-circle-clicked');
    });

    // Gestion des éléments cliquables
    const clickables = document.querySelectorAll('a, button, input, textarea, select, [role="button"], .logo, .project, .radio-container label, .radio-container input');

    clickables.forEach(el => {
        el.addEventListener('mouseenter', () => {
            cursor.classList.add('cursor-hover');
        });
        
        el.addEventListener('mouseleave', () => {
            cursor.classList.remove('cursor-hover');
        });
    });
});



