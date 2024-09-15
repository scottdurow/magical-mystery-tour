import * as React from 'react';
import { FiMaximize2 } from 'react-icons/fi'; // Import the fullscreen icon
import { Swiper, SwiperClass, SwiperSlide } from 'swiper/react';
import 'swiper/css';
import 'swiper/css/free-mode';
import 'swiper/css/navigation';
import 'swiper/css/thumbs';
import { FreeMode, Navigation, Thumbs, Zoom } from 'swiper/modules';

export interface ISliderPhoto {
    src: string;
    title: string;
}

export interface ImageSliderProps {
    images: ISliderPhoto[];
    width: number;
    height: number;
}

type CustomCSSProperties = {
    '--swiper-navigation-color': string;
    '--swiper-pagination-color': string;
};

export const ImageGridDetail = ({ width, height, images }: ImageSliderProps) => {
    const [thumbsSwiper, setThumbsSwiper] = React.useState<SwiperClass>();

    const swiperRef = React.useRef<HTMLDivElement>(null);

    const goFullScreen = () => {
        const swiperNode = swiperRef.current;

        if (swiperNode) {
            if (document.fullscreenElement) {
                document.exitFullscreen();
            } else {
                swiperNode.requestFullscreen();
            }
        }
    };

    const style: React.CSSProperties & CustomCSSProperties = {
        '--swiper-navigation-color': '#fff',
        '--swiper-pagination-color': '#fff',
    };

    return (
        <div
            ref={swiperRef}
            style={{
                margin: '0 auto',
                position: 'relative',
                width: width !== undefined && !isNaN(width) && width !== -1 ? `${width}px` : '100%',
                height: height !== undefined && !isNaN(height) && height !== -1 ? `${height}px` : '100%',
            }}
        >
            <button
                onClick={goFullScreen}
                style={{
                    position: 'absolute',
                    top: '10px',
                    right: '10px',
                    background: 'none',
                    border: 'none',
                    zIndex: 1000,
                }}
            >
                <FiMaximize2 size={32} color="white" />
            </button>
            <Swiper
                style={style}
                loop={true}
                spaceBetween={11}
                navigation={true}
                thumbs={{ swiper: thumbsSwiper }}
                modules={[Zoom, FreeMode, Navigation, Thumbs]}
                className="image-grid"
            >
                {
                    // Map the images to the SwiperSlide component and populate the src attribute with the image URL
                    images &&
                        images.map((image, index) => (
                            <SwiperSlide key={index}>
                                <img src={image.src} />
                            </SwiperSlide>
                        ))
                }
            </Swiper>
            <Swiper
                onSwiper={setThumbsSwiper}
                loop={true}
                spaceBetween={10}
                slidesPerView={4}
                freeMode={true}
                watchSlidesProgress={true}
                modules={[FreeMode, Navigation, Thumbs]}
                className="image-grid-thumbs"
            >
                {
                    // Map the images to the SwiperSlide component and populate the src attribute with the image URL
                    images &&
                        images.map((image, index) => (
                            <SwiperSlide key={index}>
                                <img src={image.src} />
                            </SwiperSlide>
                        ))
                }
            </Swiper>
        </div>
    );
};
