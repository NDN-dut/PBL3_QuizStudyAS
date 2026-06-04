document.addEventListener("DOMContentLoaded", function () {
    const chatBox = document.getElementById("chat-box");
    const chatInput = document.getElementById("chat-input");
    const btnSend = document.getElementById("btn-send");
    const mascot = document.getElementById("ai-mascot");

    function scrollToBottom() {
        chatBox.scrollTop = chatBox.scrollHeight;
    }

    // Hàm tạo và chèn bong bóng tin nhắn (ĐÃ ĐƯỢC REFACTOR ĐỊNH DẠNG)
    function appendMessage(sender, message) {
        const messageDiv = document.createElement("div");
        messageDiv.className = `d-flex mb-4 ${sender === 'user' ? 'justify-content-end' : 'justify-content-start'}`;

        const bubbleDiv = document.createElement("div");
        bubbleDiv.className = sender === 'user'
            ? "bg-primary text-white shadow-sm rounded-3 py-2 px-3"
            : "bg-white border shadow-sm rounded-3 py-2 px-3 text-dark";
        bubbleDiv.style.maxWidth = "75%";
        bubbleDiv.style.borderRadius = "0.5rem";

        if (sender === 'user') {
            bubbleDiv.style.borderBottomRightRadius = "0";
            // Tin nhắn của user: Chỉ hiển thị text thuần túy
            bubbleDiv.innerText = message;
        } else {
            bubbleDiv.style.borderBottomLeftRadius = "0";
            // Tin nhắn của AI: Dịch Markdown sang HTML
            bubbleDiv.innerHTML = marked.parse(message);

            // Yêu cầu MathJax vẽ lại công thức toán học nếu có
            if (window.MathJax) {
                MathJax.typesetPromise([bubbleDiv]).catch(function (err) {
                    console.error("Lỗi vẽ toán:", err);
                });
            }
        }

        messageDiv.appendChild(bubbleDiv);
        chatBox.appendChild(messageDiv);
        scrollToBottom();
    }

    async function sendMessage() {
        const message = chatInput.value.trim();
        if (!message) return;

        appendMessage('user', message);
        chatInput.value = "";
        chatInput.disabled = true;
        btnSend.disabled = true;

        mascot.classList.add("scared");

        const loadingId = "loading-" + Date.now();
        const loadingDiv = document.createElement("div");
        loadingDiv.id = loadingId;
        loadingDiv.className = "d-flex mb-4 justify-content-start";
        loadingDiv.innerHTML = `<div class="bg-light border text-muted shadow-sm rounded-3 py-2 px-3" style="border-bottom-left-radius: 0 !important;"><i class="bi bi-three-dots translate-middle-y" style="animation: pulse 1s infinite;"></i> Đang suy nghĩ...</div>`;
        chatBox.appendChild(loadingDiv);
        scrollToBottom();

        try {
            const response = await fetch('/Chatbot/SendMessage', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ message: message })
            });

            const data = await response.json();

            document.getElementById(loadingId).remove();

            if (data.success) {
                appendMessage('ai', data.data);
            } else {
                appendMessage('ai', `<span class="text-danger"><i class="bi bi-exclamation-triangle"></i> Lỗi: ${data.message}</span>`);
            }
        } catch (error) {
            document.getElementById(loadingId).remove();
            appendMessage('ai', `<span class="text-danger"><i class="bi bi-wifi-off"></i> Lỗi kết nối mạng.</span>`);
        } finally {
            mascot.classList.remove("scared");
            chatInput.disabled = false;
            btnSend.disabled = false;
            chatInput.focus();
        }
    }

    btnSend.addEventListener("click", sendMessage);
    chatInput.addEventListener("keypress", function (e) {
        if (e.key === "Enter") sendMessage();
    });
});