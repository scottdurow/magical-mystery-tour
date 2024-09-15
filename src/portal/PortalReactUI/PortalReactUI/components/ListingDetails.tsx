/* eslint-disable @typescript-eslint/no-explicit-any */
import * as React from 'react';
import { Row, Col, Container, Button } from 'react-bootstrap';
import { Swiper, SwiperClass, SwiperSlide } from 'swiper/react';
import 'swiper/css';
import 'swiper/css/free-mode';
import 'swiper/css/navigation';
import 'swiper/css/thumbs';
import { FreeMode, Navigation, Thumbs, Zoom } from 'swiper/modules';
import { XrmContextDataverseClient } from 'dataverse-ify';
import {
    contoso_ListingImageAttributes,
    contoso_listingimageMetadata,
} from '../dataverse-gen/entities/contoso_ListingImage';
import '../css/slider.css';
import 'react-datepicker/dist/react-datepicker.css';
import * as ReservationApi from '../api/listingsApi';
import { ListingData } from '../model/listings';
import ReservationForm from './ReservationForm';
import { navigateToPage } from '../api/navigation';

export interface ListingsProps {
    serviceClient: XrmContextDataverseClient;
}

const ListingDetails: React.FC<ListingsProps> = ({ serviceClient }) => {
    const [isLoading, setIsLoading] = React.useState(true);
    const [listingData, setListingData] = React.useState<ListingData>();

    const getListingDetails = React.useCallback(async () => {
        // Get the listing
        // Get the id parameter from the query string
        const urlParams = new URLSearchParams(window.location.search);
        const listingId = urlParams.get('id');
        if (listingId) {
            const listing = await ReservationApi.getListing(serviceClient, listingId);
            setListingData(listing);
            setIsLoading(false);
        }
    }, [serviceClient]);

    React.useEffect(() => {
        getListingDetails();
    }, [getListingDetails]);

    const [thumbsSwiper, setThumbsSwiper] = React.useState<SwiperClass>();

    const slideImages = listingData?.images.map((image) => {
        const imageDownloadUrl = `/Image/download.aspx?entity=${contoso_listingimageMetadata.logicalName}&attribute=${contoso_ListingImageAttributes.contoso_Image}&ID=${image.contoso_listingimageid}&Full=true`;
        return (
            <SwiperSlide key={image.contoso_imageid}>
                <img src={imageDownloadUrl} />
            </SwiperSlide>
        );
    });

    return (
        <Container className="py-0 bg-light-2">
            <Row className="justify-content-center mt-1">
                <Col xs={10} md={10} lg={10} xl={7} className="text-black product-details">
                    {isLoading ? (
                        <>
                            <p className="card-text placeholder-glow swiper">
                                <span className="placeholder col-7 image-grid " style={{ width: '100%' }}></span>
                                <span
                                    className="placeholder col-7 image-grid-thumbs"
                                    style={{ width: '33%', marginTop: 2 }}
                                ></span>
                            </p>
                        </>
                    ) : (
                        <>
                            <Swiper
                                style={{
                                    ['--swiper-navigation-color' as any]: '#fff',
                                    ['--swiper-pagination-color' as any]: '#fff',
                                }}
                                loop={true}
                                spaceBetween={10}
                                navigation={true}
                                thumbs={{ swiper: thumbsSwiper }}
                                modules={[Zoom, FreeMode, Navigation, Thumbs]}
                                className="image-grid"
                            >
                                {slideImages}
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
                                {slideImages}
                            </Swiper>
                        </>
                    )}
                </Col>
                <Col xs={10} md={10} lg={10} xl={5} className="text-black product-details">
                    {isLoading ? (
                        <>
                            <p className="card-text placeholder-glow">
                                <span className="placeholder col-7"></span>
                                <span className="placeholder col-4"></span>
                                <span className="placeholder col-4"></span>
                                <span className="placeholder col-6"></span>
                                <span className="placeholder col-8"></span>
                            </p>
                        </>
                    ) : (
                        <>
                            <h1>{listingData?.listing?.contoso_name}</h1>
                            {window.CRE_CURRENT_USER_ID !== '' && listingData?.listing.contoso_listingid1 && (
                                <ReservationForm listingId={listingData?.listing.contoso_listingid1} />
                            )}

                            {window.CRE_CURRENT_USER_ID === '' && (
                                <Button
                                    variant="primary"
                                    className="my-3"
                                    onClick={() => {
                                        navigateToPage(
                                            '/SignIn?ReturnUrl=' +
                                                encodeURIComponent(window.location.pathname + window.location.search),
                                        );
                                    }}
                                >
                                    Login to Reserve
                                </Button>
                            )}
                            <br />
                            <b className="text-dark-primary h4 mt-3 d-block">
                                ${listingData?.listing?.contoso_pricepermonth}/month
                            </b>
                            <br />
                            <b className="h5">4.1 ⭐⭐⭐⭐</b>
                            <p className="mt-3 h5" style={{ opacity: '0.8', fontWeight: '400' }}>
                                {listingData?.listing?.contoso_description}
                            </p>
                        </>
                    )}
                </Col>
            </Row>
        </Container>
    );
};

export default ListingDetails;
