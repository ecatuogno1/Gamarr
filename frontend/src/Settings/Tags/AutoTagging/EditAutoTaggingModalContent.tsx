import classNames from 'classnames';
import React, { useCallback, useEffect, useRef, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import AppState from 'App/State/AppState';
import Alert from 'Components/Alert';
import Card from 'Components/Card';
import Form from 'Components/Form/Form';
import FormInput from 'Components/Form/FormInput';
import FormInputHelpText from 'Components/Form/FormInputHelpText';
import FormLabel from 'Components/Form/FormLabel';
import FormRow from 'Components/Form/FormRow';
import Icon from 'Components/Icon';
import Button from 'Components/Link/Button';
import SpinnerErrorButton from 'Components/Link/SpinnerErrorButton';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { icons, inputTypes, kinds } from 'Helpers/Props';
import {
  cloneAutoTaggingSpecification,
  deleteAutoTaggingSpecification,
  fetchAutoTaggingSpecifications,
  saveAutoTagging,
  setAutoTaggingValue,
} from 'Store/Actions/settingsActions';
import { createProviderSettingsSelectorHook } from 'Store/Selectors/createProviderSettingsSelector';
import { InputChanged } from 'typings/inputs';
import translate from 'Utilities/String/translate';
import AddSpecificationModal from './Specifications/AddSpecificationModal';
import EditSpecificationModal from './Specifications/EditSpecificationModal';
import Specification from './Specifications/Specification';
import styles from './EditAutoTaggingModalContent.css';

export interface EditAutoTaggingModalContentProps {
  id?: number;
  tagsFromId?: number;
  onModalClose: () => void;
  onDeleteAutoTaggingPress?: () => void;
}

export default function EditAutoTaggingModalContent({
  id,
  tagsFromId,
  onModalClose,
  onDeleteAutoTaggingPress,
}: EditAutoTaggingModalContentProps) {
  const {
    error,
    item,
    isFetching,
    isSaving,
    saveError,
    validationErrors,
    validationWarnings,
  } = useSelector(createProviderSettingsSelectorHook('autoTaggings', id));

  const { isPopulated: specificationsPopulated, items: specifications } =
    useSelector((state: AppState) => state.settings.autoTaggingSpecifications);

  const dispatch = useDispatch();
  const [isAddSpecificationModalOpen, setIsAddSpecificationModalOpen] =
    useState(false);
  const [isEditSpecificationModalOpen, setIsEditSpecificationModalOpen] =
    useState(false);

  const handleAddSpecificationPress = useCallback(() => {
    setIsAddSpecificationModalOpen(true);
  }, [setIsAddSpecificationModalOpen]);

  const handleAddSpecificationModalClose = useCallback(
    ({ specificationSelected = false } = {}) => {
      setIsAddSpecificationModalOpen(false);
      setIsEditSpecificationModalOpen(specificationSelected);
    },
    [setIsAddSpecificationModalOpen]
  );

  const handleEditSpecificationModalClose = useCallback(() => {
    setIsEditSpecificationModalOpen(false);
  }, [setIsEditSpecificationModalOpen]);

  const handleInputChange = useCallback(
    ({ name, value }: InputChanged) => {
      // @ts-expect-error - actions are not typed
      dispatch(setAutoTaggingValue({ name, value }));
    },
    [dispatch]
  );

  const handleSavePress = useCallback(() => {
    dispatch(saveAutoTagging({ id }));
  }, [dispatch, id]);

  const handleCloneSpecificationPress = useCallback(
    (specId: number) => {
      dispatch(cloneAutoTaggingSpecification({ id: specId }));
    },
    [dispatch]
  );

  const handleConfirmDeleteSpecification = useCallback(
    (specId: number) => {
      dispatch(deleteAutoTaggingSpecification({ id: specId }));
    },
    [dispatch]
  );

  useEffect(() => {
    dispatch(fetchAutoTaggingSpecifications({ id: tagsFromId || id }));
  }, [id, tagsFromId, dispatch]);

  const isSavingRef = useRef(false);

  useEffect(() => {
    if (isSavingRef.current && !isSaving && !saveError) {
      onModalClose();
    }

    isSavingRef.current = isSaving;
  }, [isSaving, saveError, onModalClose]);

  const { name, removeTagsAutomatically, tags } = item;

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader subtitle={id ? name.previousValue ?? name.value : undefined}>
        {id ? translate('EditAutoTag') : translate('AddAutoTag')}
      </ModalHeader>

      <ModalBody>
        <div>
          {isFetching ? <LoadingIndicator /> : null}

          {!isFetching && !!error ? (
            <Alert kind={kinds.DANGER}>{translate('AddAutoTagError')}</Alert>
          ) : null}

          {!isFetching && !error && specificationsPopulated ? (
            <div>
              <Form
                validationErrors={validationErrors}
                validationWarnings={validationWarnings}
              >
                <FormRow>
                  <FormLabel>{translate('Name')}</FormLabel>

                  <FormInput
                    type={inputTypes.TEXT}
                    name="name"
                    {...name}
                    onChange={handleInputChange}
                  />
                </FormRow>

                <FormRow>
                  <FormLabel>{translate('RemoveTagsAutomatically')}</FormLabel>

                  <FormInputHelpText
                    text={translate('RemoveTagsAutomaticallyHelpText')}
                  />

                  <FormInput
                    type={inputTypes.CHECK}
                    name="removeTagsAutomatically"
                    {...removeTagsAutomatically}
                    onChange={handleInputChange}
                  />
                </FormRow>

                <FormRow>
                  <FormLabel>{translate('Tags')}</FormLabel>

                  <FormInput
                    type={inputTypes.TAG}
                    name="tags"
                    onChange={handleInputChange}
                    {...tags}
                  />
                </FormRow>
              </Form>

              <div className={styles.conditions}>
                <h3 className={styles.conditionsTitle}>
                  {translate('Conditions')}
                </h3>

                <div className={styles.autoTaggings}>
                  {specifications.map((specification) => {
                    return (
                      <Specification
                        key={specification.id}
                        {...specification}
                        onCloneSpecificationPress={
                          handleCloneSpecificationPress
                        }
                        onConfirmDeleteSpecification={
                          handleConfirmDeleteSpecification
                        }
                      />
                    );
                  })}

                  <Card
                    className={classNames(
                      styles.addSpecification,
                      !specifications.length && styles.fullWidth
                    )}
                    aria-label={translate('AddCondition')}
                    onPress={handleAddSpecificationPress}
                  >
                    <div className={styles.center}>
                      <Icon name={icons.ADD} size={20} />
                    </div>

                    <div className={styles.addLabel}>
                      {translate('AddCondition')}
                    </div>
                  </Card>
                </div>
              </div>

              <AddSpecificationModal
                isOpen={isAddSpecificationModalOpen}
                onModalClose={handleAddSpecificationModalClose}
              />

              <EditSpecificationModal
                isOpen={isEditSpecificationModalOpen}
                onModalClose={handleEditSpecificationModalClose}
              />
            </div>
          ) : null}
        </div>
      </ModalBody>
      <ModalFooter>
        <div className={styles.rightButtons}>
          {id ? (
            <Button
              className={styles.deleteButton}
              kind={kinds.DANGER}
              onPress={onDeleteAutoTaggingPress}
            >
              {translate('Delete')}
            </Button>
          ) : null}
        </div>

        <Button onPress={onModalClose}>{translate('Cancel')}</Button>

        <SpinnerErrorButton
          isSpinning={isSaving}
          error={saveError}
          onPress={handleSavePress}
        >
          {translate('Save')}
        </SpinnerErrorButton>
      </ModalFooter>
    </ModalContent>
  );
}
