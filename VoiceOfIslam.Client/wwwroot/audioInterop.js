export async function playAudio(audioElement, speed = 1.0) {
    if (audioElement) {
        audioElement.playbackRate = speed || 1.0;
        await audioElement.play();
    }
}

export async function pauseAudio(audioElement) {
    if (audioElement) {
        await audioElement.pause();
    }
}

export async function getCurrentTime(audioElement) {
    return audioElement ? audioElement.currentTime : 0;
}

export async function getDuration(audioElement) {
    return audioElement ? audioElement.duration : 0;
}

export async function setCurrentTime(audioElement, time) {
    if (audioElement) {
        audioElement.currentTime = time;
    }
}
