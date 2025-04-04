// Variable pour contrôler la puissance de l'effet scatter
let scatterPower = 0.8; // Valeur par défaut, peut être ajustée entre 0 et 3

// Fonction pour traiter les éléments avec la classe 'rotate-text'
function processRotateText(element) {
    const text = element.textContent;
    const fragment = document.createDocumentFragment();
    const container = document.createElement('span');
    container.className = 'scatter-container';
    fragment.appendChild(container);

    // Diviser le texte en mots pour éviter les coupures
    const words = text.split(/(\s+)/);

    for (let i = 0; i < words.length; i++) {
        const word = words[i];

        if (word.trim() === '') {
            // Préserver les espaces
            container.appendChild(document.createTextNode(word));
        } else {
            // Créer un conteneur pour chaque mot
            const wordSpan = document.createElement('span');
            wordSpan.className = 'scatter-word';
            wordSpan.style.display = 'inline-block';
            wordSpan.style.whiteSpace = 'nowrap';

            // Traiter chaque lettre du mot
            for (let j = 0; j < word.length; j++) {
                const span = document.createElement('span');
                span.textContent = word[j];
                span.style.display = 'inline-block';
                span.style.transition = 'transform 0.2s ease-out';
                span.className = 'rotated-letter';

                wordSpan.appendChild(span);
            }

            container.appendChild(wordSpan);
        }
    }

    element.innerHTML = ''; // Vider le contenu original
    element.appendChild(container); // Ajouter le nouveau contenu
}

// Appeler la fonction pour traiter tous les éléments avec la classe 'rotate-text' lors du chargement
document.addEventListener('DOMContentLoaded', function () {
    const textElements = document.querySelectorAll('.text-scatter');

    textElements.forEach(function (element) {
        // Vérifier si la classe 'rotate-text' est présente
        if (element.classList.contains('rotate-text')) {
            processRotateText(element); // Appeler la fonction pour découper les lettres
        }
    });
});

// Ajout de l'effet d'écartement des lettres au passage de la souris
document.addEventListener('mousemove', function (e) {
    const textElements = document.querySelectorAll('.text-scatter');

    textElements.forEach(function (element) {
        // Vérifier si la classe 'rotate-text' est présente
        if (element.classList.contains('rotate-text')) {
            processRotateText(element); // Appeler la nouvelle fonction
        }

        const rect = element.getBoundingClientRect();

        // Vérifier si la souris est sur ou proche du texte
        if (e.clientX >= rect.left &&
            e.clientX <= rect.right &&
            e.clientY >= rect.top &&
            e.clientY <= rect.bottom) {

            // Si le texte n'a pas encore été préparé pour l'effet
            if (!element.dataset.prepared) {
                // Sauvegarder le contenu HTML original
                element.dataset.originalContent = element.innerHTML;

                // Sauvegarder les styles originaux importants
                const computedStyle = window.getComputedStyle(element);
                element.dataset.originalBackground = computedStyle.background;
                element.dataset.originalWebkitBackgroundClip = computedStyle.webkitBackgroundClip;
                element.dataset.originalWebkitTextFillColor = computedStyle.webkitTextFillColor;
                element.dataset.originalColor = computedStyle.color;
                element.dataset.originalFontSize = computedStyle.fontSize;
                element.dataset.originalFontWeight = computedStyle.fontWeight;
                element.dataset.originalTextDecoration = computedStyle.textDecoration;
                element.dataset.originalTextDecorationColor = computedStyle.textDecorationColor;
                element.dataset.originalTextDecorationStyle = computedStyle.textDecorationStyle;
                element.dataset.originalTextDecorationThickness = computedStyle.textDecorationThickness;

                // Sauvegarder les classes originales pour préserver les pseudo-éléments
                element.dataset.originalClasses = element.className;

                // Fonction récursive pour traiter les nœuds de texte
                function processNode(node, parent) {
                    if (node.nodeType === Node.TEXT_NODE && node.textContent.trim() !== '') {
                        // Traiter uniquement les nœuds de texte non vides
                        const text = node.textContent;
                        const fragment = document.createDocumentFragment();

                        // Créer un conteneur pour maintenir le soulignement
                        const container = document.createElement('span');
                        container.style.textDecoration = element.dataset.originalTextDecoration;
                        container.style.textDecorationColor = element.dataset.originalTextDecorationColor;
                        container.style.textDecorationStyle = element.dataset.originalTextDecorationStyle;
                        container.style.textDecorationThickness = element.dataset.originalTextDecorationThickness;
                        container.className = 'scatter-container';
                        fragment.appendChild(container);

                        // Diviser le texte en mots pour éviter les coupures
                        const words = text.split(/(\s+)/);

                        for (let i = 0; i < words.length; i++) {
                            const word = words[i];

                            if (word.trim() === '') {
                                // Préserver les espaces
                                container.appendChild(document.createTextNode(word));
                            } else {
                                // Créer un conteneur pour chaque mot
                                const wordSpan = document.createElement('span');
                                wordSpan.className = 'scatter-word';
                                wordSpan.style.display = 'inline-block';
                                wordSpan.style.whiteSpace = 'nowrap';

                                // Traiter chaque lettre du mot
                                for (let j = 0; j < word.length; j++) {
                                    const span = document.createElement('span');
                                    span.textContent = word[j];
                                    span.style.display = 'inline-block';
                                    span.style.transition = 'transform 0.2s ease-out';
                                    span.className = 'scatter-letter';
                                    // Désactiver le soulignement sur les lettres individuelles
                                    span.style.textDecoration = 'none';

                                    // Appliquer les styles du parent au span pour les titres avec dégradé
                                    if (element.classList.contains('title')) {
                                        span.style.background = element.dataset.originalBackground;
                                        span.style.webkitBackgroundClip = element.dataset.originalWebkitBackgroundClip;
                                        span.style.webkitTextFillColor = element.dataset.originalWebkitTextFillColor;
                                        span.style.color = element.dataset.originalColor;
                                        span.style.fontWeight = element.dataset.originalFontWeight;
                                    }

                                    wordSpan.appendChild(span);
                                }

                                container.appendChild(wordSpan);
                            }
                        }

                        parent.replaceChild(fragment, node);
                    } else if (node.nodeType === Node.ELEMENT_NODE) {
                        // Si c'est un élément SVG ou autre élément spécial, ne pas le modifier
                        if (node.tagName.toLowerCase() === 'svg' ||
                            node.tagName.toLowerCase() === 'br' ||
                            node.tagName.toLowerCase() === 'img') {
                            return;
                        }

                        // Sauvegarder les styles de décoration de cet élément
                        const nodeStyle = window.getComputedStyle(node);
                        const originalTextDecoration = nodeStyle.textDecoration;
                        const originalTextDecorationColor = nodeStyle.textDecorationColor;
                        const originalTextDecorationStyle = nodeStyle.textDecorationStyle;
                        const originalTextDecorationThickness = nodeStyle.textDecorationThickness;

                        // Sauvegarder les classes pour les pseudo-éléments
                        if (node.className) {
                            node.dataset.originalNodeClasses = node.className;
                        }

                        // Sinon, traiter récursivement les enfants
                        const childNodes = Array.from(node.childNodes);
                        childNodes.forEach(child => {
                            // Passer les styles de décoration au processeur
                            node.dataset.textDecoration = originalTextDecoration;
                            node.dataset.textDecorationColor = originalTextDecorationColor;
                            node.dataset.textDecorationStyle = originalTextDecorationStyle;
                            node.dataset.textDecorationThickness = originalTextDecorationThickness;
                            processNode(child, node);
                        });
                    }
                }

                // Cloner l'élément pour le traiter
                const clone = element.cloneNode(true);
                Array.from(clone.childNodes).forEach(child => processNode(child, clone));

                // Remplacer le contenu
                element.innerHTML = clone.innerHTML;

                // Si c'est un titre avec dégradé, supprimer les styles du parent
                // car ils sont maintenant appliqués aux spans individuels
                if (element.classList.contains('title')) {
                    element.style.background = 'none';
                    element.style.webkitBackgroundClip = 'initial';
                    element.style.webkitTextFillColor = 'initial';
                }

                // Ajouter une classe spéciale pour préserver les pseudo-éléments
                element.classList.add('scatter-active');

                element.dataset.prepared = 'true';
            }

            // Marquer l'élément comme actif
            element.dataset.active = 'true';

            // Réinitialiser le timer de restauration
            if (element.resetTimer) {
                clearTimeout(element.resetTimer);
                element.resetTimer = null;
            }

            // Appliquer l'effet à chaque lettre
            const letters = element.querySelectorAll('.scatter-letter');
            letters.forEach(function (letter) {
                const letterRect = letter.getBoundingClientRect();
                const letterCenter = {
                    x: letterRect.left + letterRect.width / 2,
                    y: letterRect.top + letterRect.height / 2
                };

                // Calculer la distance entre la souris et la lettre
                const dx = e.clientX - letterCenter.x;
                const dy = e.clientY - letterCenter.y;
                const distance = Math.sqrt(dx * dx + dy * dy);

                // Rayon d'influence - adapté à la taille du texte
                const fontSize = parseFloat(element.dataset.originalFontSize || window.getComputedStyle(element).fontSize);
                const radius = Math.max(100, fontSize * 2); // Rayon minimum de 100px, ou 2x la taille de police

                if (distance < radius) {
                    // Force inversement proportionnelle à la distance
                    // Ajustée en fonction de la taille du texte et de la puissance configurée
                    const force = (1 - distance / radius) * Math.min(15, fontSize / 3) * scatterPower;

                    // Direction opposée à la souris
                    const angle = Math.atan2(dy, dx);
                    const moveX = -Math.cos(angle) * force;
                    const moveY = -Math.sin(angle) * force;

                    letter.style.transform = `translate(${moveX}px, ${moveY}px)`;
                } else {
                    letter.style.transform = 'translate(0, 0)';
                }
            });
        } else if (element.dataset.active === 'true') {
            // Marquer l'élément comme inactif
            element.dataset.active = 'false';

            // Réinitialiser les transformations en douceur
            const letters = element.querySelectorAll('.scatter-letter');
            letters.forEach(function (letter) {
                // Augmenter le temps de transition pour une sortie plus douce
                letter.style.transition = 'transform 0.5s ease-out';
                letter.style.transform = 'translate(0, 0)';

                // Remettre la transition normale après l'animation
                setTimeout(() => {
                    letter.style.transition = 'transform 0.2s ease-out';
                }, 100);
            });

            // Vérifier si la souris est suffisamment loin pour restaurer le contenu original
            const distanceX = Math.min(Math.abs(e.clientX - rect.left), Math.abs(e.clientX - rect.right));
            const distanceY = Math.min(Math.abs(e.clientY - rect.top), Math.abs(e.clientY - rect.bottom));
            const distance = Math.sqrt(distanceX * distanceX + distanceY * distanceY);

            // Distance de restauration adaptée à la taille du texte
            const fontSize = parseFloat(element.dataset.originalFontSize || window.getComputedStyle(element).fontSize);
            const resetDistance = Math.max(150, fontSize * 3);

            if (distance > resetDistance) {
                // Attendre que l'animation de sortie soit terminée avant de restaurer
                if (!element.resetTimer) {
                    element.resetTimer = setTimeout(() => {
                        // Restaurer le contenu HTML original
                        if (element.dataset.originalContent) {
                            element.innerHTML = element.dataset.originalContent;

                            // Restaurer les styles originaux pour les titres avec dégradé
                            if (element.classList.contains('title') && element.dataset.originalBackground) {
                                element.style.background = element.dataset.originalBackground;
                                element.style.webkitBackgroundClip = element.dataset.originalWebkitBackgroundClip;
                                element.style.webkitTextFillColor = element.dataset.originalWebkitTextFillColor;
                            }

                            // Restaurer les classes originales pour les pseudo-éléments
                            if (element.dataset.originalClasses) {
                                element.className = element.dataset.originalClasses;
                            }

                            delete element.dataset.prepared;
                            delete element.dataset.active;
                            element.resetTimer = null;
                        }
                    }, 600); // Attendre un peu plus que la durée de la transition
                }
            }
        }
    });
});

// Préparation des éléments pour l'effet
document.querySelectorAll('.text-scatter').forEach(function (element) {
    // Sauvegarder la couleur originale
    const originalColor = window.getComputedStyle(element).color;
    element.dataset.originalColor = originalColor;
});

// Ajouter une transition pour un effet plus fluide
const style = document.createElement('style');
style.innerHTML = `
  .text-scatter {
    transition: all 0.1s ease-out;
  }
  .scatter-container {
    display: inline-block;
    position: relative;
  }
  
  .scatter-word {
    white-space: nowrap;
    display: inline-block;
  }
  
  /* Préserver les pseudo-éléments pendant l'animation */
  .scatter-active::before, .scatter-active::after {
    position: absolute;
    z-index: -1;
  }
  
  /* Style pour les boutons avec l'effet scatter */
  span span::after {
    transform: none !important;
    transition: none !important;
    height: 0 !important;
    opacity: 0 !important;
    visibility: hidden !important;
  }
`;
document.head.appendChild(style);
