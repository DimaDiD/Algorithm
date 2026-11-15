import { notification } from 'antd';

const notificationMessage = (type, text, icon = null) => {
    (notification[type])({
        message: "Повідомлення",
        icon: icon,
        description: text
    });
};

export default notificationMessage;