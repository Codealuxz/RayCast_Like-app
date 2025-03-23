function generateGrainTexture() {
    let canvas = document.createElement("canvas");
    let ctx = canvas.getContext("2d");
    canvas.width = 1500;
    canvas.height = 1500;

    let imageData = ctx.createImageData(canvas.width, canvas.height);
    let buffer = new Uint32Array(imageData.data.buffer);

    for (let i = 0; i < buffer.length; i++) {
        let shade = Math.random() * 255;
        buffer[i] = (255 << 24) | (shade << 16) | (shade << 8) | shade;
    }

    ctx.putImageData(imageData, 0, 0);

    // Appliquer en CSS sur .first::after
    let grainTexture = canvas.toDataURL();
    let styleTag = document.createElement("style");
    styleTag.innerHTML = `
        .first::after {
            content: "";
            position: absolute;
            width: 100%;
            height: 300vh;
            top: 0;
            left: 0;
            background-image: url('${grainTexture}');
            background-size: cover;
            opacity: 1; /* Ajuste l'intensité du grain */
            mix-blend-mode: multiply;
            filter: brightness(0.8);
            -webkit-backdrop-filter: blur(5px);
            backdrop-filter: blur(5px);
            pointer-events: none;
            z-index: -1;
        }
    `;
    document.head.appendChild(styleTag);
}

generateGrainTexture();

// Fonction pour afficher le projet correspondant au radio sélectionné avec animation
function showSelectedProject() {
    // Récupérer tous les boutons radio
    const radioButtons = document.querySelectorAll('.radio-container input[type="radio"]');
    // Récupérer tous les projets
    const projectItems = document.querySelectorAll('.project-item');

    // Vérifier si les éléments existent
    if (radioButtons.length === 0 || projectItems.length === 0) {
        console.warn("Éléments radio ou projets non trouvés dans le DOM");
        return;
    }

    // S'assurer que le nombre de boutons radio correspond au nombre de projets
    if (radioButtons.length !== projectItems.length) {
        console.warn(`Nombre de boutons radio (${radioButtons.length}) différent du nombre de projets (${projectItems.length})`);
    }

    // Variable pour stocker l'index du projet actuellement affiché
    let currentIndex = 0;

    // Déterminer l'index initial (radio sélectionné par défaut)
    let radioChecked = false;
    radioButtons.forEach((radio, index) => {
        if (radio.checked) {
            currentIndex = index;
            radioChecked = true;
        }
    });

    // Si aucun radio n'est sélectionné, sélectionner le premier
    if (!radioChecked && radioButtons.length > 0) {
        radioButtons[0].checked = true;
    }

    // Configurer les styles pour les transitions
    projectItems.forEach(item => {
        // Préserver les styles originaux tout en ajoutant les propriétés nécessaires pour l'animation
        item.style.transition = 'opacity 0.5s ease, filter 0.5s ease';
        item.style.position = 'relative';
        item.style.opacity = '0';
        item.style.filter = 'blur(8px)'; // Commencer avec du flou
        item.style.display = 'none';
    });

    // Initialiser l'affichage du premier projet
    if (projectItems[currentIndex]) {
        projectItems[currentIndex].style.display = 'initial';
        // Forcer un reflow avant de changer l'opacité
        void projectItems[currentIndex].offsetWidth;

        // Appliquer l'animation d'apparition au premier élément
        setTimeout(() => {
            projectItems[currentIndex].style.opacity = '1';
            projectItems[currentIndex].style.filter = 'blur(0)';
        }, 50);
    }

    // Parcourir tous les boutons radio
    radioButtons.forEach((radio, index) => {
        // Ajouter un écouteur d'événement pour le changement
        radio.addEventListener('change', function () {
            if (currentIndex === index || index >= projectItems.length) return; // Vérification de sécurité

            const previousIndex = currentIndex;
            currentIndex = index;

            // Vérifier que les indices sont valides
            if (!projectItems[previousIndex] || !projectItems[currentIndex]) {
                console.error("Indice de projet invalide");
                return;
            }

            // Masquer le projet précédent avec effet de désintégration
            const previousProject = projectItems[previousIndex];
            previousProject.style.opacity = '0';
            previousProject.style.filter = 'blur(8px)';

            // Préparer le nouveau projet pour l'animation
            const newProject = projectItems[currentIndex];

            // Attendre que l'animation de disparition soit terminée avant de commencer l'apparition
            setTimeout(() => {
                // Masquer complètement l'ancien projet
                previousProject.style.display = 'none';

                // Préparer le nouveau projet
                newProject.style.display = 'initial';
                newProject.style.opacity = '0';
                newProject.style.filter = 'blur(8px)';

                // Forcer un reflow pour que la transition fonctionne
                void newProject.offsetWidth;

                // Déclencher l'animation d'apparition
                setTimeout(() => {
                    newProject.style.opacity = '1';
                    newProject.style.filter = 'blur(0)';
                }, 50);
            }, 500);
        });
    });
}

// Exécuter la fonction lorsque le DOM est complètement chargé
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', showSelectedProject);
} else {
    // Si le DOM est déjà chargé (par exemple, si le script est chargé de manière asynchrone)
    showSelectedProject();
}

