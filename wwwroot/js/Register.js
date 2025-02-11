


const tags = [
    {
        emoji: '🐶',
        name: 'dog'
    }
    , {
        emoji: '🐱',
        name: 'cat'
    }
    , {
        emoji: '🐭',
        name: 'mouse'
    }
    , {
        emoji: '🐹',
        name: 'hamster'
    }
    , {
        emoji: '🐰',
        name: 'rabbit'
    }
    , {
        emoji: '🦊',
        name: 'fox'
    }
    , {
        emoji: '🐻',
        name: 'bear'
    }
    , {
        emoji: '🐼',
        name: 'panda'
    }
    , {
        emoji: '🐨',
        name: 'koala'
    }
    , {
        emoji: '🐯',
        name: 'tiger'
    }
    , {
        emoji: '🦁',
        name: 'lion'
    }
    , {
        emoji: '🐷',
        name: 'pig'
    }
    , {
        emoji: '🐸',
        name: 'frog'
    }
    , {
        emoji: '🐵',
        name: 'monkey'
    }
    , {
        emoji: '🐔',
        name: 'chicken'
    }
    , {
        emoji: '🦆',
        name: 'duck'
    }
    , {
        emoji: '🦅',
        name: 'eagle'
    }
    , {
        emoji: '🦉',
        name: 'owl'
    }
    , {
        emoji: '🦇',
        name: 'bat'
    }
    , {
        emoji: '🐺',
        name: 'wolf'
    }
]

document.addEventListener('DOMContentLoaded', () => {
    const image = document.getElementById('profileImage');
    const imageInput = document.querySelector('input[type="file"]');
    const div_tags = document.getElementById('tags');
    localStorage.setItem('tags_selected', '[]');
    imageInput.addEventListener('change', (e) => {
        console.log('input changed');
        image.src = URL.createObjectURL(e.target.files[0]);
    });
    
    tags.forEach(tag => {
        const div = document.createElement('div');
        div.className = 'tag';
        div.style = 'display: inline-block; margin: 3px; padding: 4px 8px 4px 8px ; border: 2px solid var(--text-gray-color); border-radius: 30px; cursor: pointer;';
        div.onclick = () =>{
            if (div.name === 'selected'){
                div.style.backgroundColor = 'transparent';
                div.style.border = '2px solid var(--text-gray-color)';
                div.name = '';
                localStorage.setItem('tags_selected', JSON.stringify(JSON.parse(localStorage.getItem('tags_selected')).filter(t => t !== tag.name)));
                return;
            }
            else{
                div.style.backgroundColor = 'var(--pink-color)';
                div.style.border = '2px solid var(--pink-color)';
                div.name = 'selected';
                localStorage.setItem('tags_selected', JSON.stringify([...JSON.parse(localStorage.getItem('tags_selected')), tag.name]));
            }
        }
    
        div.innerHTML = tag.emoji + ' ' + tag.name;
        div_tags.appendChild(div);
    });
});
