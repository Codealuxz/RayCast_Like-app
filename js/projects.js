function openPopup(title, description, images, isGame = false, isWebsite = false, visitUrl = '', playUrl = '', downloadUrl = '', tags = []) {
    const url = new URL(window.location);
    url.searchParams.set('project', title);
    window.history.pushState({}, '', url);

    document.body.classList.add('no-scroll');
    document.getElementById('popup-title').innerText = title;
    document.getElementById('popup-title').classList.add('title-popup');
    document.getElementById('popup-description').innerText = description;
    document.getElementById('popup-description').classList.add('description-popup');
    const imagesContainer = document.getElementById('popup-images');
    imagesContainer.classList.add('images-popup');
    imagesContainer.innerHTML = '';
    images.forEach(img => {
        const imgElement = document.createElement('img');
        imgElement.src = img;
        imagesContainer.appendChild(imgElement);
    });

    const tagsContainer = document.getElementById('tag-contener');
    tagsContainer.innerHTML = '';
    tags.forEach(tag => {
        const tagElement = document.createElement('span');
        tagElement.classList.add('tag');
        tagElement.innerText = tag;
        tagsContainer.appendChild(tagElement);
    });

    const buttonsContainer = document.getElementById('popup-buttons');
    buttonsContainer.innerHTML = '';

    if (isWebsite) {
        const visitButton = document.createElement('button');
        visitButton.innerText = 'Visiter';
        visitButton.classList.add('project');
        visitButton.onclick = () => window.open(visitUrl, '_blank');
        buttonsContainer.appendChild(visitButton);
    }

    if (isGame) {
        const playButton = document.createElement('button');
        playButton.innerText = 'Jouer';
        playButton.classList.add('project');
        playButton.onclick = () => window.open(playUrl, '_blank');
        buttonsContainer.appendChild(playButton);
    }

    if (title === 'Spita') {
        const downloadButton = document.createElement('button');
        downloadButton.innerText = 'Télécharger';
        downloadButton.classList.add('project');
        downloadButton.onclick = () => window.open(downloadUrl, '_blank');
        buttonsContainer.appendChild(downloadButton);
    }

    document.getElementById('project-popup').style.left = '0';
}

const projects = {
    "Codealuxz photo": {
        description: "This is a website for my photography. I expose my works and share them.",
        images: ["img/photo.png"],
        isGame: false,
        isWebsite: true,
        visitUrl: "https://photo.codealuxz.fr",
        tags: ["HTML", "CSS", "JavaScript"]
    },
    "Pyde": {
        description: "Pyde is a website how you can run python code in the browser and make your own programs it is a online IDE for python.",
        images: ["img/pyde.png", "img/pyde_ui.png"],
        isGame: false,
        isWebsite: true,
        visitUrl: "https://openedide.heberking.com",
        tags: ["HTML", "CSS", "JavaScript", "Node.js", "Express"]
    },
    "Chess4stats": {
        description: "This is a website made for a chess club. It's for creating groups and the user can join it and be convened.",
        images: ["img/chess.png"],
        isGame: false,
        isWebsite: false,
        visitUrl: '',
        tags: ["HTML", "CSS", "JavaScript", "Firebase"]
    },
    "Pixels-board": {
        description: "This is a website that recreates the experience of a r/place (the pixel war) but with an infinite canvas and lots of tools.",
        images: ["img/board.png"],
        isGame: false,
        isWebsite: true,
        visitUrl: "https://pixels-board.codealuxz.fr",
        tags: ["HTML", "CSS", "JavaScript", "Node.js", "Express", "Socket.io"]
    },
    "Portfolio (old)": {
        description: "This is the first version of my old portfolio.",
        images: ["img/portfolioV1.png"],
        isGame: false,
        isWebsite: true,
        visitUrl: "https://v1.codealuxz.fr",
        tags: ["HTML", "CSS", "JavaScript"]
    },
    "Protonix": {
        description: "This is a game made with Godot engine. The goal is to avoid the enemies in a 2D world with 3 different modes and grind on the world leaderboard.",
        images: ["img/protonix.png"],
        isGame: true,
        isWebsite: false,
        playUrl: "https://codealuxz.itch.io/protonix",
        tags: ["Godot Engine", "GDscript", "Firebase"]
    },
    "Spita": {
        description: "This is a game made with Godot engine. This is a 2D platformer with a lot of levels and a lot of enemies.",
        images: ["img/spita.png"],
        isGame: true,
        isWebsite: false,
        playUrl: "https://codealuxz.itch.io/spita",
        downloadUrl: "https://codealuxz.itch.io/spitaexe",
        tags: ["Godot Engine", "GDscript"]
    }
};

document.addEventListener('DOMContentLoaded', () => {
    const urlParams = new URLSearchParams(window.location.search);
    const projectId = urlParams.get('project');

    if (projectId && projects[projectId]) {
        const project = projects[projectId];
        openPopup(
            projectId,
            project.description,
            project.images,
            project.isGame,
            project.isWebsite,
            project.visitUrl,
            project.playUrl,
            project.downloadUrl,
            project.tags
        );
    }
});

function closePopup() {
    document.getElementById('project-popup').style.left = '-100%';
    document.body.classList.remove('no-scroll');

    const url = new URL(window.location);
    url.searchParams.delete('project');
    window.history.pushState({}, '', url);
}

window.onclick = function (event) {
    if (event.target === document.getElementById('project-popup')) {
        closePopup();
    }
}

document.onkeydown = function (event) {
    if (event.key === "Escape") {
        closePopup();
    }
}